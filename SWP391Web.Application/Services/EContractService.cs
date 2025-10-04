using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Pdf;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.ValueObjects;
using SWP391Web.Infrastructure.IRepository;
using System.ComponentModel.Design;
using System.Text;
using System.Text.Json;
using UglyToad.PdfPig;

namespace SWP391Web.Application.Services
{
    public class EContractService : IEContractService
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private readonly IVnptEContractClient _vnpt;
        private readonly IUnitOfWork _unitOfWork;
        public EContractService(IConfiguration cfg, HttpClient http, IUnitOfWork unitOfWork, IVnptEContractClient vnpt)
        {
            _cfg = cfg;
            _http = http;
            _unitOfWork = unitOfWork;
            _vnpt = vnpt;
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

        public async Task<ResponseDTO> CreateEContractAsync(CreateDealerDTO createDealerDTO, CancellationToken ct)
        {
            try
            {
                //    // 1) Load dealer + manager
                //    var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(dto.DealerId, ct);
                //    if (dealer is null) return new ResponseDTO { IsSuccess = false, StatusCode = 404, Message = "Dealer not exist" };

                //    var manager = await _unitOfWork.DealerMemberRepository.GetManagerAsync(dto.DealerId, ct);
                //    if (manager is null) return new ResponseDTO { IsSuccess = false, StatusCode = 400, Message = "Dealer has not DealerManager" };


                //    // 2) Render PDF (QuestPDF) — mock company info
                //    var companyName = "EV Manufacturer Sample"; // sample
                //    using var pdf = DealerContractPdf.Render(companyName, dealer.Name, dealer.Address, dealer.Email + ", " + dealer.PhoneNumber, DateTime.Now);
                // 1) Load dealer + manager

                var isExistDealer = await _unitOfWork.DealerRepository.IsExistByNameAsync(createDealerDTO.DealerName, ct);
                if (isExistDealer)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Dealer name is exist"
                    };

                var isEmailExist = await _unitOfWork.UserManagerRepository.IsEmailExist(createDealerDTO.EmailManager);
                if (isEmailExist)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Email is exist"
                    };
                }

                var dealer = new Dealer
                {
                    Id = Guid.NewGuid(),
                    Name = createDealerDTO.DealerName,
                    Address = createDealerDTO.DealerAddress,
                };

                var user = new ApplicationUser
                {
                    FullName = createDealerDTO.FullNameManager,
                    Email = createDealerDTO.EmailManager,
                    PhoneNumber = createDealerDTO.PhoneNumberManager
                };


                // 3) Auth VNPT
                var token = await GetAccessTokenAsync();

                var vnptUserCode = $"dealer.{dealer.Id:N}.manager";
                var vnptUser = new VnptUserUpsert
                {
                    Code = vnptUserCode,
                    UserName = user.Email,
                    Name = user.FullName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
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

                var created = await CreateDocumentAsync(token, dealer, user);

                var companyApproverUserCode = _cfg["SmartCA:CompanyApproverUserCode"] ?? throw new ArgumentNullException("SmartCA:CompanyApproverUserCode is not exist");

                var documentId = created.Data?.Id;
                if (documentId is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "VNPT did not return document ID."
                    };

                if (created.Data?.PositionA is null || created.Data.PositionB is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "VNPT did not return signature positions."
                    };

                var uProcess = await UpdateProcessAsync(token, documentId, companyApproverUserCode, vnptUserCode, created.Data.PositionA, created.Data.PositionB);

                var sent = await SendProcessAsync(token, documentId);

                await _unitOfWork.DealerRepository.AddAsync(dealer, ct);
                var randomPassword = "Dealer@" + Guid.NewGuid().ToString()[..5];
                await _unitOfWork.UserManagerRepository.CreateAsync(user, randomPassword);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "PDF is created",
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

        private async Task<VnptResult<VnptDocumentDto>> CreateDocumentAsync(string token, Dealer dealer, ApplicationUser user)
        {

            // 2) Render PDF (QuestPDF) — mock company info
            var companyName = "EV Manufacturer Sample"; // sample
            using var pdf = DealerContractPdf.Render(companyName, dealer.Name, dealer.Address, user.Email + ", " + user.PhoneNumber, DateTime.Now);
            var documentTypeId = 3059;
            var departmentId = 3110;
            var bytes = pdf.ToArray();
            var pageA = DealerContractPdf.FindAnchorBox(bytes, "ĐẠI_DIỆN_BÊN_A");
            var pageB = DealerContractPdf.FindAnchorBox(bytes, "ĐẠI_DIỆN_BÊN_B");

            double weight = 170, hight = 90, offsetY = 36, margin = 18;
            using var ms = new MemoryStream(bytes);
            using var doc = PdfDocument.Open(ms);
            var page = doc.GetPage(pageA.Page);
            double pw = page.Width;

            double llxA = Math.Clamp(pageA.Left, margin, pw - margin - weight);
            double llyA = Math.Max(pageA.Bottom - offsetY - hight, margin);
            string posA = $"{(int)llxA - 28},{(int)llyA},{(int)(llxA + weight - 28)},{(int)(llyA + hight)}";

            double llxB = Math.Clamp(pageB.Left, margin, pw - margin - weight);
            double llyB = Math.Max(pageB.Bottom - offsetY - hight, margin);
            string posB = $"{(int)llxB},{(int)llyB},{(int)(llxB + weight)},{(int)(llyB + hight)}";

            var randomText = Guid.NewGuid().ToString()[..5].ToUpper();
            var request = new CreateDocumentDTO
            {
                TypeId = documentTypeId,
                DepartmentId = departmentId,
                No = $"EContract-{randomText}",
                Subject = $"Test-{randomText}",
                Description = "Test"
            };

            request.FileInfo.File = pdf.ToArray();
            request.FileInfo.FileName = $"EContract-{randomText}.pdf";

            var createResult = await _vnpt.CreateDocumentAsync(token, request);
            createResult.Data!.PositionA = posA;
            createResult.Data.PositionB = posB;
            Console.WriteLine($"CreateDocument: {createResult.Messages[0]}");
            return createResult;
        }

        private async Task<VnptResult<VnptDocumentDto>> UpdateProcessAsync(string token, string documentId, string userCodeFirst, string userCodeSeccond, string positionA, string positionB )
        {
            var request = new VnptUpdateProcessDTO
            {
                Id = documentId,
                ProcessInOrder = true,
                Processes =
                [
                    new (orderNo:1, processedByUserCode:userCodeFirst, accessPermissionCode:"D", position: positionA, pageSign: 1),
                    new (orderNo:2, processedByUserCode:userCodeSeccond, accessPermissionCode:"D", position: positionB, pageSign: 1)
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

        public async Task<ResponseDTO> SignProcess(string token, VnptProcessDTO vnptProcessDTO)
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

        public async Task<HttpResponseMessage> GetPreviewResponseAsync(string token, string? rangeHeader = null, CancellationToken ct = default)
            => await _vnpt.GetDownloadResponseAsync(token, rangeHeader, ct);

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
    }
}

