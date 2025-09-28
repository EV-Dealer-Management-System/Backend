using Microsoft.Extensions.Configuration;
using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Pdf;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Domain.ValueObjects;
using SWP391Web.Infrastructure.IRepository;
using SWP391Web.Infrastructure.Repository;
using System.Text;
using System.Text.Json;

namespace SWP391Web.Application.Services
{
    public class EContractService : IEContractService
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private readonly IVnptEContractClient _vnpt;
        private readonly JsonSerializerOptions _jon = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
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

        public async Task<ResponseDTO> CreateAndSendAsync(CreateDealerDTO createDealerDTO)
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

                var randomPassword = "Dealer@" + Guid.NewGuid().ToString()[..5];
                await _unitOfWork.UserManagerRepository.CreateAsync(user, randomPassword);

                var manager = new DealerMember
                {
                    UserId = user.Id,
                    DealerId = dealer.Id,
                    User = user
                };


                // 3) Auth VNPT
                var token = await GetAccessTokenAsync();

                var vnptUserCode = $"dealer.{dealer.Id:N}.manager";
                var vnptUser = new VnptUserUpsert
                {
                    Code = vnptUserCode,
                    UserName = manager.User.Email,
                    Name = manager.User.FullName,
                    Email = manager.User.Email,
                    Phone = manager.User.PhoneNumber,
                    ReceiveOtpMethod = 1,
                    ReceiveNotificationMethod = 0,
                    SignMethod = 2,
                    SignConfirmationEnabled = true,
                    GenerateSelfSignedCertEnabled = false,
                    Status = 1
                };

                var vnptUserList = new[] { vnptUser };

                var upsert = await CreateOrUpdateUsersAsync(token, vnptUserList);

                var created = await CreateDocumentAsync(token, dealer, manager);

                var companyApproverUserCode = _cfg["SmartCA:CompanyApproverUserCode"] ?? throw new ArgumentNullException("SmartCA:CompanyApproverUserCode is not exist");

                var documentId = created.Data?.Id;
                if (documentId is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "VNPT did not return document ID."
                    };
                var uProcess = await UpdateProcessAsync(token, documentId, companyApproverUserCode, vnptUserCode);

                var sent = await SendProcessAsync(token, documentId);

                //    // 4) Ensure dealer manager exists in VNPT as a user
                //    var userCode = $"dealer.{dealer.DealerId:N}.manager";
                //    await _vnpt.CreateOrUpdateUsersAsync(token, new[]
                //    {
                //        new VnptUserUpsert(
                //        Code: userCode,
                //        UserName: manager.User.Email,
                //        Name: manager.User.FullName,
                //        Email: manager.User.Email,
                //        Phone: manager.User.PhoneNumber,
                //        ReceiveOtpMethod: 1, // email
                //        ReceiveNotificationMethod: 0, // email
                //        SignMethod: 2, // SmartCA (2) — tùy đổi 1 = draw
                //        SignConfirmationEnabled: true,
                //        GenerateSelfSignedCertEnabled: true,
                //        Status: 1
                //    )}, ct);


                //    // 5) Create document on VNPT
                //    var docNo = string.IsNullOrWhiteSpace(dto.DocumentNo) ? $"HDDL-{DateTime.UtcNow:yyyyMMddHHmmss}" : dto.DocumentNo!;
                //    var subject = string.IsNullOrWhiteSpace(dto.Subject) ? $"HĐ Đại lý – {dealer.Name}" : dto.Subject!;
                //    var typeId = dto.TypeId ?? int.Parse(_cfg["SmartCA:TypeId"] ?? "001");
                //    var departmentId = dto.DepartmentId ?? int.Parse(_cfg["SmartCA:DepartmentId"] ?? "001");

                //    var created = await _vnpt.CreateDocumentAsync(token,
                //        new VnptCreateDocReq(
                //            No: docNo,
                //            Subject: subject,
                //            Description: dto.Description,
                //            TypeId: typeId,
                //            DepartmentId: departmentId
                //        ),
                //        pdf, $"{docNo}.pdf", ct);

                //    // 6) Update workflow: dealer manager first, company approver second
                //    var approver = string.IsNullOrWhiteSpace(dto.CompanyApproverUserCode)
                //        ? _cfg["SmartCA:CompanyApproverUserCode"] ?? "hoangtuzami"
                //        : dto.CompanyApproverUserCode!;

                //    var updated = await _vnpt.UpdateProcessAsync(token,
                //        new VnptUpdateProcessReq(
                //            Id: created.Id,
                //            ProcessInOrder: true,
                //            Processes: new List<VnptProcessItem>
                //            {
                //                new(OrderNo:1, ProcessedByUserCode:approver, AccessPermissionCode:"D"),
                //                new(OrderNo:2, ProcessedByUserCode:userCode, AccessPermissionCode:dto.DealerPermissionCode)
                //            }
                //        ), ct);
                //    // 7) Send process
                //    var sent = await _vnpt.SendProcessAsync(token, updated.Id, ct);


                //    return new ResponseDTO
                //    {
                //        IsSuccess = true,
                //        StatusCode = 201,
                //        Message = "Đã tạo PDF, khởi tạo quy trình ký và gửi cho DealerManager.",
                //        Result = new { vnptDocumentId = sent.Id, vnptNo = sent.No, vnptStatus = sent.Status.Description }
                //    };
                //}
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Đã tạo PDF, khởi tạo quy trình ký và gửi cho DealerManager.",
                    Result = sent
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Lỗi gửi HĐ đại lý: {ex.Message}"
                };
            }
        }

        private async Task<VnptResult<VnptDocumentDto>> CreateDocumentAsync(string token, Dealer dealer, DealerMember manager)
        {

            // 2) Render PDF (QuestPDF) — mock company info
            var companyName = "EV Manufacturer Sample"; // sample
            using var pdf = DealerContractPdf.Render(companyName, dealer.Name, dealer.Address, manager.User.Email + ", " + manager.User.PhoneNumber, DateTime.Now);
            var documentTypeId = 3059;
            var departmentId = 3110;

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
            Console.WriteLine($"CreateDocument: {createResult.Messages[0]}");
            return createResult;
        }

        private async Task<VnptResult<VnptDocumentDto>> UpdateProcessAsync(string token, string documentId, string userCodeFirst, string userCodeSeccond)
        {
            var request = new VnptUpdateProcessDTO
            {
                Id = documentId,
                ProcessInOrder = true,
                Processes =
                [
                    new (orderNo:1, processedByUserCode:userCodeFirst, accessPermissionCode:"D", position: "14,478,206,568", pageSign: 1),
                    new (orderNo:2, processedByUserCode:userCodeSeccond, accessPermissionCode:"D", position: "14,678,206,768", pageSign: 1)
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

        public async Task<ResponseDTO> SignProcess(VnptProcessDTO vnptProcessDTO)
        {
            try
            {
                var token = await GetAccessTokenAsync();

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
                    ConfirmTermsConditions = vnptProcessDTO.ConfirmTermsConditions
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
    }
}

