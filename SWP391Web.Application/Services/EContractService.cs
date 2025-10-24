using AutoMapper;
using EVManagementSystem.Application.DTO.EContract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EContract;
using SWP391Web.Application.DTO.Warehouse;
using SWP391Web.Application.IService;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Pdf;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Domain.ValueObjects;
using SWP391Web.Infrastructure.IRepository;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;
using UglyToad.PdfPig;

namespace SWP391Web.Application.Services
{
    public class EContractService : IEContractService
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private readonly IVnptEContractClient _vnpt;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IS3Service _s3Service;
        private readonly IWarehouseService _warehouseService;
        public EContractService(IWarehouseService warehouseService, IConfiguration cfg, HttpClient http, IUnitOfWork unitOfWork, IVnptEContractClient vnpt, IEmailService emailService, IMapper mapper, IS3Service s3Service)
        {
            _cfg = cfg;
            _http = http;
            _unitOfWork = unitOfWork;
            _vnpt = vnpt;
            _emailService = emailService;
            _mapper = mapper;
            _s3Service = s3Service;
            _warehouseService = warehouseService;
        }

        public async Task<ResponseDTO<GetAccessTokenDTO>> GetAccessTokenAsync()
        {
            var username = _cfg["EContractClient:Username"] ?? throw new Exception("Cannot find username in EContractClient");
            var password = _cfg["EContractClient:Password"] ?? throw new Exception("Cannot find password in EContractClient");
            int? companyId = _cfg["EContractClient:CompanyId"] is not null ? int.Parse(_cfg["EContractClient:CompanyId"]!) : throw new Exception("Cannot find company ID in EContractClient");

            var payload = new { username, password, companyId };
            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var urlGetToken = $"{_cfg["EContractClient:BaseUrl"]}/api/auth/password-login";
            using var req = new HttpRequestMessage(HttpMethod.Post, urlGetToken);
            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return new ResponseDTO<GetAccessTokenDTO>
                {
                    IsSuccess = false,
                    StatusCode = (int)res.StatusCode,
                    Message = $"Cannot get access token from EContract: {body}"
                };

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var dataEl = root.GetProperty("data");
            string? accessToken = dataEl.ValueKind == JsonValueKind.String ? dataEl.GetString() :
            (dataEl.ValueKind == JsonValueKind.Object && dataEl.TryGetProperty("access", out var t1)) ? t1.GetString() :
            (dataEl.ValueKind == JsonValueKind.Object && dataEl.TryGetProperty("accessToken", out var t2)) ? t2.GetString() :
            null;

            if (string.IsNullOrWhiteSpace(accessToken))
                return new ResponseDTO<GetAccessTokenDTO>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = "Cannot find access token in EContract response"
                };

            var userId = int.Parse(_cfg["EContractClient:UserId"] ?? throw new Exception("Cannot find user ID in EContractClient"));

            return new ResponseDTO<GetAccessTokenDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get access token successfully",
                Data = new GetAccessTokenDTO
                {
                    AccessToken = accessToken,
                    UserId = userId
                }
            };
        }

        public async Task<ResponseDTO> CreateBookingEContractAsync(ClaimsPrincipal userClaim, Guid bookingId, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User is not login yet"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer is not exist"
                    };
                }

                var access = await GetAccessTokenAsync();

                var created = await CreateDocumentBookingAsync(bookingId, access.Data.AccessToken, dealer, ct);

                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(created.Data!.Id), ct);

                var companyName = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist");
                var supportEmail = _cfg["Company:Email"] ?? throw new ArgumentNullException("Company:Email is not exist");

                var companyApproverUserCode = _cfg["EContractClient:CompanyApproverUserCode"] ?? throw new ArgumentNullException("SmartCA:CompanyApproverUserCode is not exist");
                await UpdateProcessAsync(access.Data.AccessToken, created.Data.Id, userId, companyApproverUserCode, created.Data.PositionA, created.Data.PositionB, created.Data.PageSign);
                var result = await SendProcessAsync(access.Data.AccessToken, created.Data.Id);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "PDF is created",
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create EContract: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateDraftEContractAsync(ClaimsPrincipal userClaim, CreateDealerDTO createDealerDTO, CancellationToken ct)
        {
            try
            {
                var isExistDealer = await _unitOfWork.DealerRepository.IsExistByNameAsync(createDealerDTO.DealerName, ct);
                if (isExistDealer)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Dealer name is exist"
                    };

                var user = new ApplicationUser
                {
                    UserName = createDealerDTO.EmailManager,
                    FullName = createDealerDTO.FullNameManager,
                    Email = createDealerDTO.EmailManager,
                    PhoneNumber = createDealerDTO.PhoneNumberManager,
                    LockoutEnabled = true
                };

                var dealer = new Dealer
                {
                    Id = Guid.NewGuid(),
                    DealerLevel = createDealerDTO.DealerLevel,
                    ManagerId = user.Id,
                    Name = createDealerDTO.DealerName,
                    Address = createDealerDTO.DealerAddress,
                    TaxNo = createDealerDTO.TaxNo,

                    Manager = user
                };

                var access = await GetAccessTokenAsync();

                var created = await CreateDocumentDealerAsync(userClaim, access.Data!.AccessToken, dealer, user, createDealerDTO.AdditionalTerm, createDealerDTO.RegionDealer, ct);

                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(created.Data!.Id), ct);

                await _unitOfWork.UserManagerRepository.CreateAsync(user, "ChangeMe@" + Guid.NewGuid().ToString()[..5]);
                await _unitOfWork.DealerRepository.AddAsync(dealer, ct);

                var companyName = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist");
                var supportEmail = _cfg["Company:Email"] ?? throw new ArgumentNullException("Company:Email is not exist");

                var encode = HttpUtility.UrlEncode(created.Data.DownloadUrl);
                var url = StaticLinkUrl.ViewDaftEContractURL + encode;
                await _emailService.SendContractEmailAsync(user.Email, user.FullName, created.Data!.Subject, url, created.Data.PdfBytes, created.Data.FileName, companyName, supportEmail);

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "PDF is created",
                    Result = created
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create EContract: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateEContractAsync(ClaimsPrincipal userClaim, Guid eContractId, CancellationToken ct)
        {
            try
            {
                var eContract = await _unitOfWork.EContractRepository.GetByIdAsync(eContractId, ct);
                if (eContract is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Cannot find EContract"
                    };

                var access = await GetAccessTokenAsync();

                var dealerManagerId = eContract.OwnerBy;

                var dealerManager = await _unitOfWork.UserManagerRepository.GetByIdAsync(dealerManagerId);
                if (dealerManager is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Cannot find dealer manager"
                    };

                var roleId = new List<Guid> 
                { 
                    Guid.Parse(_cfg["EContract:RoleId"] ?? throw new Exception("EContract:RoleId is not exist")) 
                };

                var vnptUser = new VnptUserUpsert
                {
                    Code = dealerManagerId,
                    UserName = dealerManager.Email,
                    Name = dealerManager.FullName,
                    Email = dealerManager.Email,
                    Phone = dealerManager.PhoneNumber,
                    ReceiveOtpMethod = 1,
                    ReceiveNotificationMethod = 0,
                    SignMethod = 2,
                    SignConfirmationEnabled = true,
                    GenerateSelfSignedCertEnabled = false,
                    Status = 1,
                    DepartmentIds = [4106],
                    RoleIds = roleId

                };

                var vnptUserList = new[] { vnptUser };

                var upsert = await CreateOrUpdateUsersAsync(access.Data!.AccessToken, vnptUserList);

                var companyApproverUserCode = _cfg["EContractClient:CompanyApproverUserCode"] ?? throw new ArgumentNullException("EContractClient:CompanyApproverUserCode is not exist");

                var draftEContract = await GetVnptEContractByIdAsync(eContractId.ToString(), ct);
                if (!draftEContract.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Cannot get draft EContract from VNPT"
                    };
                }

                var uProcess = await UpdateProcessAsync(access.Data!.AccessToken, eContractId.ToString(), companyApproverUserCode, dealerManagerId, draftEContract.Data!.PositionA!, draftEContract.Data!.PositionA!, draftEContract.Data.PageSign);

                var sent = await SendProcessAsync(access.Data!.AccessToken, eContractId.ToString());


                if (!Enum.IsDefined(typeof(EContractStatus), sent.Data.Status.Value))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid EContract status value.",
                    };
                }

                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(eContractId, ct);
                if (econtract is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "EContract not found.",
                    };
                }

                econtract.UpdateStatus((EContractStatus)sent.Data.Status.Value);

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Econtract ready to sign",
                    Result = sent
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create EContract: {ex.Message}"
                };
            }
        }

        public static (string, int) GetVnptEContractPosition(byte[] pdfBytes, AnchorBox anchor,
        double width = 170, double height = 90, double offsetY = 36,
        double margin = 18, double xAdjust = 0)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var doc = PdfDocument.Open(ms);
            var page = doc.GetPage(anchor.Page);
            var lastPage = doc.NumberOfPages;

            double pw = page.Width;

            var llx = Math.Clamp(anchor.Left + xAdjust, margin, pw - margin - width);
            var lly = Math.Max(anchor.Bottom - offsetY - height, margin);

            var pos = $"{(int)llx},{(int)lly},{(int)(llx + width)},{(int)(lly + height)}";
            return (pos, lastPage);
        }

        private Dictionary<string, object?> BuildPlaceholders(
            string dealerName, string dealerAddress, string dealerTax,
            string dealerContact, string companyRole, string dealerRole,
            string companyRepresentative, string dealerRepresentative
            )
        {

            var data = new Dictionary<string, object?>
            {
                ["company.name"] = _cfg["Company:Name"] ?? "N/A",
                ["company.address"] = _cfg["Company:Address"] ?? "N/A",
                ["company.taxNo"] = _cfg["Company:TaxNo"] ?? "N/A",
                ["dealer.name"] = dealerName,
                ["dealer.address"] = dealerAddress,
                ["dealer.taxNo"] = dealerTax,
                ["dealer.contact"] = dealerContact,
                ["roles.A.representative"] = companyRepresentative,
                ["roles.A.title"] = companyRole,
                ["roles.B.representative"] = dealerRepresentative,
                ["roles.B.title"] = dealerRole,
            };

            return data;
        }


        private async Task<VnptResult<VnptDocumentDto>> CreateDocumentBookingAsync(Guid bookingId, string token, Dealer dealer, CancellationToken ct)
        {
            var templateCode = _cfg["EContract:BookingTemplateCode"] ?? throw new ArgumentNullException("EContract:DealerTemplateCode is not exist");
            var template = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(templateCode, ct);
            if (template is null) throw new Exception($"Template with code '{templateCode}' is not exist");

            var booking = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingId);
            if (booking is null)
            {
                throw new Exception($"Booking with id '{bookingId}' is not exist");
            }

            var bookingDetails = await _unitOfWork.BookingDetailRepository.GetBookingDetailsByBookingIdAsync(bookingId, ct);
            if (bookingDetails is null || !bookingDetails.Any())
            {
                throw new Exception($"Booking detail with booking id '{bookingId}' is not exist");
            }

            string BuildBookingRowsHtml(IEnumerable<BookingEVDetail> items)
            {
                var sb = new StringBuilder();
                int i = 1;
                foreach (var item in items)
                {
                    var modelName = item.Version?.Model?.ModelName ?? "(Mẫu)";
                    var versionName = item.Version?.VersionName ?? "(Phiên bản)";
                    var colorName = item.Color?.ColorName ?? "(Màu)";
                    var quantity = item.Quantity;

                    sb.AppendLine($@"
                        <tr>
                        <td class=""right"">{i}</td>
                        <td>{modelName} – {versionName}</td>
                        <td>{colorName}</td>
                        <td class=""right"">{quantity}</td>
                        </tr>");
                    i++;
                }
                return sb.ToString();
            }

            var rowsHtml = BuildBookingRowsHtml(bookingDetails);
            var totalQty = booking.TotalQuantity;

            var data = new Dictionary<string, object?>
            {
                ["company.name"] = _cfg["Company:Name"] ?? "N/A",
                ["company.address"] = _cfg["Company:Address"] ?? "N/A",
                ["company.taxNo"] = _cfg["Company:TaxNo"] ?? "N/A",
                ["dealer.name"] = dealer.Name,
                ["dealer.address"] = dealer.Address,
                ["dealer.taxNo"] = dealer.TaxNo,
                ["dealer.contact"] = $"{dealer.Manager.Email}, {dealer.Manager.PhoneNumber}",
                ["booking.date"] = booking.BookingDate.ToString("dd/MM/yyyy HH:mm"),
                ["booking.total"] = totalQty.ToString(),
                ["booking.note"] = booking.Note ?? string.Empty,
                ["booking.rows"] = rowsHtml
            };

            var html = EContractPdf.ReplacePlaceholders(template.ContentHtml, data, htmlEncode: false);

            var pdfBytes = await EContractPdf.RenderAsync(html);
            var anchors = EContractPdf.FindAnchors(pdfBytes, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });

            var positionA = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
            var positionB = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);

            var documentTypeId = int.Parse(_cfg["EContract:DocumentTypeId"] ?? throw new NullReferenceException("EContract:DocumentTypeId is not exist"));
            var departmentId = int.Parse(_cfg["EContract:DepartmentId"] ?? throw new NullReferenceException("EContract:DepartmentId is not exist"));

            var randomText = Guid.NewGuid().ToString()[..6].ToUpper();

            var request = new CreateDocumentDTO
            {
                TypeId = documentTypeId,
                DepartmentId = departmentId,
                No = $"EContract-{randomText}",
                Subject = $"Booking Confirm EContract",
                Description = "EContract allows dealer confirm booking electric vehicle"
            };

            request.FileInfo.File = pdfBytes;
            var fileName = $"Booking_E-Contract_{randomText}_{dealer.Name}.pdf".Trim();
            request.FileInfo.FileName = fileName;

            var createResult = await _vnpt.CreateDocumentAsync(token, request);


            if (!Enum.IsDefined(typeof(EContractStatus), createResult.Data.Status.Value))
            {
                throw new Exception("Invalid EContract status value.");
            }

            var status = (EContractStatus)createResult.Data.Status.Value;

            var EContract = new EContract(Guid.Parse(createResult.Data.Id), template.Id, fileName, "System", dealer.ManagerId!, status, EcontractType.BookingContract);

            await _unitOfWork.EContractRepository.AddAsync(EContract, ct);
            await _unitOfWork.SaveAsync();

            createResult.Data!.PositionA = positionA.Item1;
            createResult.Data.PositionB = positionB.Item1;
            createResult.Data.PageSign = positionA.Item2;
            createResult.Data.FileName = request.FileInfo.FileName;

            return createResult;
        }

        private async Task<VnptResult<VnptDocumentDto>> CreateDocumentDealerAsync(ClaimsPrincipal userClaim, string token, Dealer dealer, ApplicationUser user, string? additional, string? regionDealer, CancellationToken ct)
        {
            var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) throw new Exception("The user is not login yet");

            var templateCode = _cfg["EContract:DealerTemplateCode"] ?? throw new ArgumentNullException("EContract:DealerTemplateCode is not exist");

            var template = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(templateCode, ct);
            if (template is null) throw new Exception($"Template with code '{templateCode}' is not exist");

            var term = await _unitOfWork.EContractTermRepository.GetByLevelAsync(dealer.DealerLevel, ct);
            if (term is null) throw new Exception($"Term for dealer level '{dealer.DealerLevel}' is not exist");

            var companyName = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist");

            var data = new Dictionary<string, object?>
            {
                ["company.name"] = companyName,
                ["company.address"] = _cfg["Company:Address"] ?? "N/A",
                ["company.taxNo"] = _cfg["Company:TaxNo"] ?? "N/A",
                ["dealer.name"] = dealer.Name,
                ["dealer.address"] = dealer.Address,
                ["dealer.taxNo"] = dealer.TaxNo,
                ["dealer.contact"] = $"{user.Email}, {user.PhoneNumber}",
                ["contract.date"] = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                ["contract.effectiveDate"] = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                ["contract.expiryDate"] = DateTime.UtcNow.AddDays(365).ToString("dd/MM/yyyy"),
                ["term.scope"] = term.Scope,
                ["terms.pricing"] = term.Pricing,
                ["terms.payment"] = term.Payment,
                ["terms.commitments"] = term.Commitment,
                ["terms.region"] = regionDealer == null ? "Toàn quốc" : regionDealer,
                ["terms.noticeDays"] = term.NoticeDay,
                ["terms.orderConfirmDays"] = term.OrderConfirmDays,
                ["terms.deliveryLocation"] = term.DeliveryLocation,
                ["terms.paymentMethod"] = term.PaymentMethod,
                ["terms.paymentDueDays"] = term.PaymentDueDays,
                ["terms.penaltyRate"] = term.PenaltyRate,
                ["terms.claimDays"] = term.ClaimDays,
                ["terms.terminationNoticeDays"] = term.TerminationNoticeDays,
                ["terms.disputeLocation"] = term.DisputeLocation,
                ["roles.A.representative"] = term.RoleRepresentative,
                ["roles.A.title"] = term.RoleTitle,
                ["roles.B.representative"] = user.FullName,
                ["roles.B.title"] = "Khách hàng",
                ["additional"] = additional == null ? "Không có điều khoản bổ sung" : additional
            };

            var html = EContractPdf.ReplacePlaceholders(template.ContentHtml, data, htmlEncode: false);

            //html = EContractPdf.RenderHtml(html, term);
            var pdfBytes = await EContractPdf.RenderAsync(html);

            var anchors = EContractPdf.FindAnchors(pdfBytes, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });

            var positionA = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
            var positionB = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);

            var documentTypeId = int.Parse(_cfg["EContract:DocumentTypeId"] ?? throw new NullReferenceException("EContract:DocumentTypeId is not exist"));
            var departmentId = int.Parse(_cfg["EContract:DepartmentId"] ?? throw new NullReferenceException("EContract:DepartmentId is not exist"));

            var randomText = Guid.NewGuid().ToString()[..6].ToUpper();

            var request = new CreateDocumentDTO
            {
                TypeId = documentTypeId,
                DepartmentId = departmentId,
                No = $"EContract-{randomText}",
                Subject = $"Dealer Contract",
                Description = "Contract allows customers to open dealer"
            };

            request.FileInfo.File = pdfBytes;
            var fileName = $"Dealer_E-Contract_{randomText}_{dealer.Name}.pdf".Trim();
            request.FileInfo.FileName = fileName;

            var createResult = await _vnpt.CreateDocumentAsync(token, request);


            if (!Enum.IsDefined(typeof(EContractStatus), createResult.Data.Status.Value))
            {
                throw new Exception("Invalid EContract status value.");
            }

            var status = (EContractStatus)createResult.Data.Status.Value;

            var EContract = new EContract(Guid.Parse(createResult.Data.Id), template.Id, fileName, userId, user.Id, status, EcontractType.DealerContract);

            await _unitOfWork.EContractRepository.AddAsync(EContract, ct);

            createResult.Data!.PositionA = positionA.Item1;
            createResult.Data.PositionB = positionB.Item1;
            createResult.Data.PageSign = positionA.Item2;
            createResult.Data.FileName = request.FileInfo.FileName;

            return createResult;
        }

        private async Task<VnptResult<VnptDocumentDto>> UpdateProcessAsync(string token, string documentId, string userCodeFirst, string userCodeSeccond, string positionA, string positionB, int pageSign)
        {
            var request = new VnptUpdateProcessDTO
            {
                Id = documentId,
                ProcessInOrder = true,
                Processes =
                [
                    new (orderNo:1, processedByUserCode:userCodeFirst, accessPermissionCode:"D", position: positionA, pageSign: pageSign),
                        new (orderNo:2, processedByUserCode:userCodeSeccond, accessPermissionCode:"D", position: positionB, pageSign: pageSign)
                ]
            };

            var uProcessResult = await _vnpt.UpdateProcessAsync(token, request);
            return uProcessResult;
        }

        private async Task<VnptResult<VnptDocumentDto>> SendProcessAsync(string token, string documentId)
            => await _vnpt.SendProcessAsync(token, documentId);


        private async Task<VnptResult<List<VnptUserDto>>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users)
           => await _vnpt.CreateOrUpdateUsersAsync(token, users);


        public Task<byte[]> DownloadAsync(string url)
          => _vnpt.DownloadAsync(url);

        public async Task<ResponseDTO> SignProcess(string token, VnptProcessDTO vnptProcessDTO, CancellationToken ct)
        {
            try
            {
                var request = new VnptProcessDTO
                {
                    ProcessId = vnptProcessDTO.ProcessId,
                    Reason = vnptProcessDTO.Reason,
                    Reject = vnptProcessDTO.Reject,
                    Otp = vnptProcessDTO.Otp,
                    SignatureDisplayMode = vnptProcessDTO.SignatureDisplayMode,
                    SignatureImage = vnptProcessDTO.SignatureImage,
                    SigningPage = vnptProcessDTO.SigningPage,
                    SigningPosition = vnptProcessDTO.SigningPosition,
                    SignatureText = vnptProcessDTO.SignatureText,
                    FontSize = vnptProcessDTO.FontSize,
                    ShowReason = vnptProcessDTO.ShowReason,
                    ConfirmTermsConditions = vnptProcessDTO.ConfirmTermsConditions,
                };

                var signResult = await _vnpt.SignProcess(token, request);

                if (signResult.Data?.Status is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = string.Join("; ", signResult.Messages)
                    };
                }

                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(signResult.Data.Id, ct);
                if (econtract is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "EContract not found.",
                    };
                }

                if (!Enum.IsDefined(typeof(EContractStatus), signResult.Data.Status.Value))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid EContract status value.",
                    };
                }

                if (!signResult.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = signResult.Code == 0 ? 500 : 200,
                        Message = $"Error to digital sign: {string.Join(", ", signResult.Messages)}",
                        Result = signResult
                    };
                }

                if (signResult.Data.Status.Value == (int)EContractStatus.Completed)
                {
                    await CreateDealerAccount(signResult.Data.Id.ToString(), ct);
                    econtract.UpdateStatus(EContractStatus.Completed);
                    _unitOfWork.EContractRepository.Update(econtract);
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Success to digital sign",
                    Result = signResult
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to digital sign: {ex.Message}"
                };
            }
        }


        private async Task CreateDealerAccount(string documentId, CancellationToken ct)
        {
            var eContract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(documentId), ct);
            if (eContract is null) throw new Exception($"Cannot find EContract with id '{documentId}'");

            var dealerManager = await _unitOfWork.UserManagerRepository.GetByIdAsync(eContract.OwnerBy);
            if (dealerManager is null) throw new Exception($"Cannot find dealer manager with id '{eContract.OwnerBy}'");

            var password = "Dealer@" + Guid.NewGuid().ToString()[..6];

            dealerManager.EmailConfirmed = true;
            dealerManager.PhoneNumberConfirmed = true;
            dealerManager.LockoutEnabled = false;
            _unitOfWork.UserManagerRepository.Update(dealerManager);

            var addToRoleResult = await _unitOfWork.UserManagerRepository.AddToRoleAsync(dealerManager, StaticUserRole.DealerManager);
            if (addToRoleResult is null) throw new Exception($"Cannot add dealer manager to role '{StaticUserRole.DealerManager}'");

            await _unitOfWork.UserManagerRepository.SetPassword(dealerManager, password);
            await _unitOfWork.SaveAsync();
            var data = new Dictionary<string, string>
            {
                ["{FullName}"] = dealerManager.FullName,
                ["{UserName}"] = dealerManager.Email,
                ["{Password}"] = password,
                ["{LoginUrl}"] = StaticLinkUrl.WebUrl,
                ["{Company}"] = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist"),
                ["{SupportEmail}"] = _cfg["Company:Email"] ?? throw new ArgumentNullException("Company:Email is not exist")
            };
            await _emailService.SendEmailFromTemplate(dealerManager.Email, "DealerWelcome", data);

            var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(dealerManager.Id, ct);
            if (dealer is null) throw new Exception($"Cannot find dealer with manager id '{dealerManager.Id}'");

            var warehouse = new CreateWarehouseDTO
            {
                DealerId = dealer.Id,
                EVCInventoryId = null,
                WarehouseType = WarehouseType.Dealer,
                WarehouseName = $"Kho {dealer.Name}"
            };

            await _warehouseService.CreateWarehouseAsync(warehouse);
        }

        public async Task<HttpResponseMessage> GetPreviewResponseAsync(string downloadUrl, string? rangeHeader = null, CancellationToken ct = default)
        {
            try
            {
                return await _vnpt.GetDownloadResponseAsync(downloadUrl, rangeHeader, ct);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error to get preview response: {ex.Message}");
            }
        }

        public async Task<ProcessLoginInfoDto> GetAccessTokenAsyncByCode(string processCode, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(processCode))
                throw new ArgumentException("processCode is required", nameof(processCode));

            var url = $"{_cfg["EContractClient:BaseUrl"]}/api/auth/process-code-login";
            var payload = new { processCode };

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            using var res = await _http.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"HTTP {(int)res.StatusCode} {res.ReasonPhrase}\n{req.Method} {req.RequestUri}\n{body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (!root.TryGetProperty("data", out var dataEl) ||
                dataEl.ValueKind == JsonValueKind.Null ||
                dataEl.ValueKind == JsonValueKind.Undefined)
            {
                if (root.TryGetProperty("messages", out var msgsEl) && msgsEl.ValueKind == JsonValueKind.Array)
                {
                    var msgs = string.Join("; ", msgsEl.EnumerateArray()
                                                       .Select(m => m.ValueKind == JsonValueKind.String ? m.GetString() : m.ToString()));
                    //throw new HttpRequestException($"HTTP {(int)res.StatusCode} {res.ReasonPhrase}\n{req.Method} {req.RequestUri}\n{body}");
                    throw new Exception("The code is used");
                }
            }

            dataEl = root.GetProperty("data");
            string? accessToken = null;
            if (dataEl.TryGetProperty("access", out var tokenEl))
            {
                if (tokenEl.ValueKind == JsonValueKind.String)
                {
                    accessToken = tokenEl.GetString();
                }
                else if (tokenEl.ValueKind == JsonValueKind.Object &&
                         tokenEl.TryGetProperty("accessToken", out var atEl) &&
                         atEl.ValueKind == JsonValueKind.String)
                {
                    accessToken = atEl.GetString();
                }
            }

            var docEl = dataEl.GetProperty("document");

            string? waitingProcessId = null;
            int? processedByUserId = null;
            string? downloadUrl = null;
            string? position = null;
            int? pageSign = null;

            if (docEl.TryGetProperty("waitingProcess", out var waitingEl))
            {
                if (waitingEl.TryGetProperty("id", out var idPro) && idPro.ValueKind == JsonValueKind.String)
                    waitingProcessId = idPro.GetString();

                if (waitingEl.TryGetProperty("processedByUserId", out var pEl) && pEl.ValueKind == JsonValueKind.Number)
                    processedByUserId = pEl.GetInt32();

                if (waitingEl.TryGetProperty("position", out var psEl) && psEl.ValueKind == JsonValueKind.String)
                    position = psEl.GetString();

                if (waitingEl.TryGetProperty("pageSign", out var pageEl) && pageEl.ValueKind == JsonValueKind.Number)
                    pageSign = pageEl.GetInt32();
            }

            if (docEl.TryGetProperty("downloadUrl", out var down) && down.ValueKind == JsonValueKind.String)
                downloadUrl = down.GetString();

            return new ProcessLoginInfoDto
            {
                ProcessId = waitingProcessId,
                DownloadUrl = downloadUrl,
                ProcessedByUserId = processedByUserId,
                AccessToken = accessToken,
                Position = position,
                PageSign = pageSign
            };
        }

        public async Task<VnptResult<VnptSmartCAResponse>> AddSmartCA(AddNewSmartCADTO addNewSmartCADTO)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.AddSmartCA(access.Data!.AccessToken, addNewSmartCADTO);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to add SmartCA: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<VnptSmartCAResponse>($"Exception when adding SmartCA: {ex.Message}");
            }
        }

        public async Task<VnptResult<VnptFullUserData>> GetSmartCAInformation(int userId)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.GetSmartCAInformation(access.Data!.AccessToken, userId);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to get SmartCA information: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<VnptFullUserData>($"Exception when getting SmartCA information: {ex.Message}");
            }
        }

        public async Task<VnptResult<VnptSmartCAResponse>> UpdateSmartCA(UpdateSmartDTO updateSmartDTO)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.UpdateSmartCA(access.Data!.AccessToken, updateSmartDTO);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to update SmartCA: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<VnptSmartCAResponse>($"Exception when updating SmartCA: {ex.Message}");
            }
        }

        public async Task<VnptResult<UpdateEContractResponse>> UpdateEContract(UpdateEContractDTO updateEContractDTO, CancellationToken ct)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var contract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(updateEContractDTO.Id), ct);
                if (contract is null)
                    return new VnptResult<UpdateEContractResponse>($"Cannot find EContract with id '{updateEContractDTO.Id}'");

                var dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(contract.OwnerBy, ct);
                if (dealer is null)
                    return new VnptResult<UpdateEContractResponse>($"Cannot find dealer with manager id '{contract.OwnerBy}'");

                var dealerManager = await _unitOfWork.UserManagerRepository.GetByIdAsync(contract.OwnerBy);
                if (dealerManager is null)
                    return new VnptResult<UpdateEContractResponse>($"Cannot find dealer manager with id '{contract.OwnerBy}'");

                var term = await _unitOfWork.EContractTermRepository.GetByLevelAsync(dealer.DealerLevel, ct);

                var data = new Dictionary<string, object?>
                {
                    ["company.name"] = _cfg["Company:Name"] ?? "N/A",
                    ["company.address"] = _cfg["Company:Address"] ?? "N/A",
                    ["company.taxNo"] = _cfg["Company:TaxNo"] ?? "N/A",
                    ["dealer.name"] = dealer.Name,
                    ["dealer.address"] = dealer.Address,
                    ["dealer.taxNo"] = dealer.TaxNo,
                    ["dealer.contact"] = $"{dealerManager.Email}, {dealerManager.PhoneNumber}",
                    ["contract.date"] = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                    ["contract.effectiveDate"] = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                    ["contract.expiryDate"] = DateTime.UtcNow.AddDays(365).ToString("dd/MM/yyyy"),
                    ["term.scope"] = term.Scope,
                    ["terms.pricing"] = term.Pricing,
                    ["terms.payment"] = term.Payment,
                    ["terms.commitments"] = term.Commitment,
                    ["terms.noticeDays"] = term.NoticeDay,
                    ["terms.orderConfirmDays"] = term.OrderConfirmDays,
                    ["terms.deliveryLocation"] = term.DeliveryLocation,
                    ["terms.paymentMethod"] = term.PaymentMethod,
                    ["terms.paymentDueDays"] = term.PaymentDueDays,
                    ["terms.penaltyRate"] = term.PenaltyRate,
                    ["terms.claimDays"] = term.ClaimDays,
                    ["terms.terminationNoticeDays"] = term.TerminationNoticeDays,
                    ["terms.disputeLocation"] = term.DisputeLocation,
                    ["roles.A.representative"] = term.RoleRepresentative,
                    ["roles.A.title"] = term.RoleTitle,
                    ["roles.B.representative"] = dealerManager.FullName,
                    ["roles.B.title"] = "Khách hàng"
                };

                var html = EContractPdf.ReplacePlaceholders(updateEContractDTO.HtmlFile, data, htmlEncode: false);
                var filePdf = await EContractPdf.RenderAsync(html);

                var formFile = new FormFile(
                    new MemoryStream(filePdf), 0, filePdf.Length, "file", updateEContractDTO.Subject + ".pdf"
                );

                var response = await _vnpt.UpdateEContract(access.Data!.AccessToken, updateEContractDTO.Id, updateEContractDTO.Subject, formFile);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to update EContract: {errors}");
                }

                var anchors = EContractPdf.FindAnchors(filePdf, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });
                var positionA = GetVnptEContractPosition(filePdf, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
                var positionB = GetVnptEContractPosition(filePdf, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);
                response.Data.PositionA = positionA.Item1;
                response.Data.PositionB = positionB.Item1;
                response.Data.PageSign = positionA.Item2;
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<UpdateEContractResponse>($"Exception when updating EContract: {ex.Message}");
            }
        }

        public async Task<ResponseDTO<EContract>> GetAllEContractList(int? pageNumber, int? pageSize, EContractStatus eContractStatus = default, EcontractType econtractType = default)
        {
            try
            {
                var eContractList = await _unitOfWork.EContractRepository.GetAllAsync(includes: e => e.Include(inc => inc.Owner));

                if (eContractStatus != default)
                {
                    eContractList = eContractList.Where(ec => ec.Status == eContractStatus);
                }

                if (econtractType != default)
                {
                    eContractList = eContractList.Where(ec => ec.Type == econtractType);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    eContractList = eContractList.Skip(((int)pageNumber - 1) * (int)pageSize).Take((int)pageSize).ToList();
                }
                else
                {
                    return new ResponseDTO<EContract>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "pageNumber and pageSize must be greater than 0"
                    };
                }

                var getList = _mapper.Map<List<GetEContractDTO>>(eContractList);

                foreach (var eContract in getList)
                {
                    eContract.CreatedName = (await _unitOfWork.UserManagerRepository.GetByIdAsync(eContract.CreatedBy))?.FullName;
                }
                return new ResponseDTO<EContract>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get EContract list successfully",
                    Result = getList
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO<EContract>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get EContract list: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<EContract>> GetEContractByIdAsync(string eContractId, CancellationToken ct)
        {
            try
            {
                var eContract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(eContractId), ct);
                if (eContract is null)
                {
                    return new ResponseDTO<EContract>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "EContract not found"
                    };
                }


                var getEContract = _mapper.Map<GetEContractDTO>(eContract);
                getEContract.CreatedName = (await _unitOfWork.UserManagerRepository.GetByIdAsync(eContract.CreatedBy))?.FullName;
                return new ResponseDTO<EContract>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get EContract successfully",
                    Result = getEContract
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO<EContract>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get EContract by id: {ex.Message}"
                };
            }
        }

        public async Task<VnptResult<VnptDocumentDto>> GetVnptEContractByIdAsync(string eContractId, CancellationToken ct)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.GetEContractByIdAsync(access.Data!.AccessToken, eContractId);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to get EContract by id: {errors}");
                }

                var filePdf = await _vnpt.DownloadAsync(response.Data!.DownloadUrl!);
                var anchors = EContractPdf.FindAnchors(filePdf, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });
                var positionA = GetVnptEContractPosition(filePdf, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
                var positionB = GetVnptEContractPosition(filePdf, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);
                response.Data.PositionA = positionA.Item1;
                response.Data.PositionB = positionB.Item1;
                response.Data.PageSign = positionA.Item2;
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<VnptDocumentDto>($"Exception when get EContract by id: {ex.Message}");
            }
        }

        public async Task<VnptResult<GetEContractResponse<DocumentListItemDto>>> GetAllVnptEContractList(int? pageNumber, int? pageSize, EContractStatus eContractStatus)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.GetEContractList(access.Data!.AccessToken, pageNumber, pageSize, eContractStatus);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to get all vnpt EContract: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<GetEContractResponse<DocumentListItemDto>>($"Exception when get all vnpt EContract: {ex.Message}");
            }
        }
    }
}

