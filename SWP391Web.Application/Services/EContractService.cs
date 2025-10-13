using EVManagementSystem.Application.DTO.EContract;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EContract;
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
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;

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
        public EContractService(IConfiguration cfg, HttpClient http, IUnitOfWork unitOfWork, IVnptEContractClient vnpt, IEmailService emailService, IMapper mapper)
        {
            _cfg = cfg;
            _http = http;
            _unitOfWork = unitOfWork;
            _vnpt = vnpt;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var username = _cfg["SmartCA:username"] ?? throw new Exception("Cannot find username in SmartCA");
            var password = _cfg["SmartCA:password"] ?? throw new Exception("Cannot find password in SmartCA");
            int? companyId = null;

            var payload = new { username, password, companyId };
            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var urlGetToken = $"{_cfg["SmartCA:BaseUrl"]}/api/v2/auth/password-login";
            using var req = new HttpRequestMessage(HttpMethod.Post, urlGetToken);
            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"login failed {(int)res.StatusCode} {res.ReasonPhrase} @ {res.RequestMessage?.RequestUri}\n{body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var dataEl = root.GetProperty("data");
            string? accessToken = dataEl.ValueKind == JsonValueKind.String ? dataEl.GetString() :
            (dataEl.ValueKind == JsonValueKind.Object && dataEl.TryGetProperty("token", out var t1)) ? t1.GetString() :
            (dataEl.ValueKind == JsonValueKind.Object && dataEl.TryGetProperty("accessToken", out var t2)) ? t2.GetString() :
            null;


            if (string.IsNullOrWhiteSpace(accessToken))
                throw new Exception($"Missing access token in response at GetAccessTokenAsync: {body}");

            return accessToken!;
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

                dealer.ApplicationUsers.Add(user);

                var token = await GetAccessTokenAsync();

                var created = await CreateDocumentPlusAsync(userClaim, token, dealer, user, createDealerDTO.AdditionalTerm, createDealerDTO.RegionDealer, ct);

                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(created.Data!.Id), ct);

                await _unitOfWork.UserManagerRepository.CreateAsync(user, "ChangeMe@" + Guid.NewGuid().ToString()[..5]);
                await _unitOfWork.DealerRepository.AddAsync(dealer, ct);


                var companyName = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist");
                var supportEmail = _cfg["Company:Email"] ?? throw new ArgumentNullException("Company:Email is not exist");

                var decodeUrl = HttpUtility.UrlDecode(created.Data!.DownloadUrl);
                await _emailService.SendContractEmailAsync(user.Email, user.FullName, created.Data!.Subject, decodeUrl, created.Data.PdfBytes, created.Data.FileName, companyName, supportEmail);

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

        public async Task<ResponseDTO> CreateEContractAsync(ClaimsPrincipal userClaim, CreateEContractDTO createEContractDTO, CancellationToken ct)
        {
            try
            {
                var eContractId = createEContractDTO.EContractId;
                var eContract = await _unitOfWork.EContractRepository.GetByIdAsync(eContractId, ct);
                if (eContract is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Cannot find EContract"
                    };

                var token = await GetAccessTokenAsync();

                var dealerManagerId = eContract.OwnerBy;

                var dealerManager = await _unitOfWork.UserManagerRepository.GetByIdAsync(dealerManagerId);
                if (dealerManager is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Cannot find dealer manager"
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
                    RoleIds = [Guid.Parse("0aa2afc9-39c5-4652-baec-08ddc28cdda2")]

                };

                var vnptUserList = new[] { vnptUser };

                var upsert = await CreateOrUpdateUsersAsync(token, vnptUserList);

                var companyApproverUserCode = _cfg["SmartCA:CompanyApproverUserCode"] ?? throw new ArgumentNullException("SmartCA:CompanyApproverUserCode is not exist");

                var uProcess = await UpdateProcessAsync(token, eContractId.ToString(), companyApproverUserCode, dealerManagerId, createEContractDTO.positionA, createEContractDTO.positionB, createEContractDTO.pageSign);

                var sent = await SendProcessAsync(token, eContractId.ToString());


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

        private async Task<VnptResult<VnptDocumentDto>> CreateDocumentPlusAsync(ClaimsPrincipal userClaim, string token, Dealer dealer, ApplicationUser user, string? additional, string? regionDealer, CancellationToken ct)
        {
            var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("The user is not login yet");

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

            var EContract = new EContract(Guid.Parse(createResult.Data.Id), template.Id, fileName, userId, user.Id, status);

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
                        Message = "Sign process failed: Missing status in response.",
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

                econtract.UpdateStatus((EContractStatus)signResult.Data.Status.Value);

                if (!signResult.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = signResult.Code == 0 ? 500 : 200,
                        Message = $"Lỗi ký số: {string.Join(", ", signResult.Messages)}",
                        Result = signResult
                    };
                }

                if (signResult.Data.Status.Value == (int)EContractStatus.Completed)
                {
                    await CreateDealerAccount(signResult.Data.Id.ToString(), ct);
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Ký số thành công.",
                    Result = signResult
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Lỗi ký số: {ex.Message}"
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

            var data = new Dictionary<string, string>
            {
                ["FullName"] = dealerManager.FullName,
                ["UserName"] = dealerManager.Email,
                ["Password"] = password,
                ["LoginUrl"] = "https://metrohcmc.xyz", // để tạm thời, config sau
                ["Company"] = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist"),
                ["SupportEmail"] = _cfg["Company:Email"] ?? throw new ArgumentNullException("Company:Email is not exist")
            };
            await _emailService.SendEmailFromTemplate(dealerManager.Email, "DealerWelcome", data);
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

            var url = $"{_cfg["SmartCA:BaseUrl"]}/api/auth/process-code-login";
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
                    throw new HttpRequestException($"HTTP {(int)res.StatusCode} {res.ReasonPhrase}\n{req.Method} {req.RequestUri}\n{body}");
                }
            }

            dataEl = root.GetProperty("data");
            string? accessToken = null;
            if (dataEl.TryGetProperty("token", out var tokenEl))
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

            if (docEl.TryGetProperty("waitingProcess", out var waitingEl))
            {
                if (waitingEl.TryGetProperty("id", out var idPro) && idPro.ValueKind == JsonValueKind.String)
                    waitingProcessId = idPro.GetString();

                if (waitingEl.TryGetProperty("processedByUserId", out var pEl) && pEl.ValueKind == JsonValueKind.Number)
                    processedByUserId = pEl.GetInt32();
            }

            if (docEl.TryGetProperty("downloadUrl", out var down) && down.ValueKind == JsonValueKind.String)
                downloadUrl = down.GetString();

            return new ProcessLoginInfoDto
            {
                ProcessId = waitingProcessId,
                DownloadUrl = downloadUrl,
                ProcessedByUserId = processedByUserId,
                AccessToken = accessToken
            };
        }

        public async Task<VnptResult<VnptSmartCAResponse>> AddSmartCA(AddNewSmartCADTO addNewSmartCADTO)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var response = await _vnpt.AddSmartCA(token, addNewSmartCADTO);
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
                var token = await GetAccessTokenAsync();
                var response = await _vnpt.GetSmartCAInformation(token, userId);
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
                var token = await GetAccessTokenAsync();
                var response = await _vnpt.UpdateSmartCA(token, updateSmartDTO);
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

        public async Task<VnptResult<UpdateEContractResponse>> UpdateEContract(UpdateEContractDTO updateEContractDTO)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var filePdf = await EContractPdf.RenderAsync(updateEContractDTO.HtmlFile);

                var formFile = new FormFile(
                    new MemoryStream(filePdf), 0, filePdf.Length, "file", updateEContractDTO.Subject + ".pdf"
                );

                var response = await _vnpt.UpdateEContract(token, updateEContractDTO.Id, updateEContractDTO.Subject, formFile);
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

        public async Task<ResponseDTO<EContract>> GetAllEContractList(int? pageNumber, int? pageSize, EContractStatus eContractStatus = default)
        {
            try
            {
                var eContractList = await _unitOfWork.EContractRepository.GetAllAsync();

                if (eContractStatus != default)
                {
                    eContractList = eContractList.Where(ec => ec.Status == eContractStatus);
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
                var token = await GetAccessTokenAsync();
                var response = await _vnpt.GetEContractByIdAsync(token, eContractId);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to get EContract by id: {errors}");
                }
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
                var token = await GetAccessTokenAsync();
                var response = await _vnpt.GetEContractList(token, pageNumber, pageSize, eContractStatus);
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

