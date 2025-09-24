using Microsoft.Extensions.Configuration;
using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Pdf;
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

        public async Task<string> GetAccessTokenAsync(CancellationToken token = default)
        {
            var username = _cfg["SmartCA:username"] ?? throw new Exception("Cannot find username in SmartCA");
            var password = _cfg["SmartCA:password"] ?? throw new Exception("Cannot find password in SmartCA");
            int? companyId = null;

            var payload = new { username, password, companyId };
            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var urlGetToken = $"{_cfg["SmartCA:BaseUrl"]}/api/auth/password-login";
            using var req = new HttpRequestMessage(HttpMethod.Post, urlGetToken);
            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req, token);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync(token);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"login failed {(int)res.StatusCode} {res.ReasonPhrase} @ {res.RequestMessage?.RequestUri}\n{body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var accessToken = root.GetProperty("data").GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new Exception("Missing access token in response at GetAccessTokenAsync");

            return accessToken;
        }

        public async Task<ResponseDTO> CreateAndSendAsync(CreateDealerContractDto dto, CancellationToken ct)
        {
            try
            {
                // 1) Load dealer + manager
                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(dto.DealerId, ct);
                if (dealer is null) return new ResponseDTO { IsSuccess = false, StatusCode = 404, Message = "Dealer không tồn tại" };

                var manager = await _unitOfWork.DealerMemberRepository.GetManagerAsync(dto.DealerId, ct);
                if (manager is null) return new ResponseDTO { IsSuccess = false, StatusCode = 400, Message = "Dealer chưa có DealerManager" };


                // 2) Render PDF (QuestPDF) — mock company info
                var companyName = "EV Manufacturer Sample"; // sample
                using var pdf = DealerContractPdf.Render(companyName, dealer.Name, dealer.Address, dealer.Email + ", " + dealer.PhoneNumber, DateTime.Now);


                // 3) Auth VNPT
                var token = await GetAccessTokenAsync();


                // 4) Ensure dealer manager exists in VNPT as a user
                var userCode = $"dealer.{dealer.DealerId:N}.manager";
                await _vnpt.CreateOrUpdateUsersAsync(token, new[]
                {
                    new VnptUserUpsert(
                    Code: userCode,
                    UserName: manager.User.Email,
                    Name: manager.User.FullName,
                    Email: manager.User.Email,
                    Phone: manager.User.PhoneNumber,
                    ReceiveOtpMethod: 1, // email
                    ReceiveNotificationMethod: 0, // email
                    SignMethod: 2, // SmartCA (2) — tùy đổi 1 = draw
                    SignConfirmationEnabled: true,
                    GenerateSelfSignedCertEnabled: true,
                    Status: 1
                )}, ct);


                // 5) Create document on VNPT
                var docNo = string.IsNullOrWhiteSpace(dto.DocumentNo) ? $"HDDL-{DateTime.UtcNow:yyyyMMddHHmmss}" : dto.DocumentNo!;
                var subject = string.IsNullOrWhiteSpace(dto.Subject) ? $"HĐ Đại lý – {dealer.Name}" : dto.Subject!;
                var typeId = dto.TypeId ?? int.Parse(_cfg["SmartCA:TypeId"] ?? "001");
                var departmentId = dto.DepartmentId ?? int.Parse(_cfg["SmartCA:DepartmentId"] ?? "001");

                var created = await _vnpt.CreateDocumentAsync(token,
                    new VnptCreateDocReq(
                        No: docNo,
                        Subject: subject,
                        Description: dto.Description,
                        TypeId: typeId,
                        DepartmentId: departmentId
                    ),
                    pdf, $"{docNo}.pdf", ct);

                // 6) Update workflow: dealer manager first, company approver second
                var approver = string.IsNullOrWhiteSpace(dto.CompanyApproverUserCode)
                    ? _cfg["SmartCA:CompanyApproverUserCode"] ?? "company.approver"
                    : dto.CompanyApproverUserCode!;

                var updated = await _vnpt.UpdateProcessAsync(token,
                    new VnptUpdateProcessReq(
                        Id: created.Id,
                        ProcessInOrder: true,
                        Processes: new List<VnptProcessItem>
                        {
                            new(OrderNo:1, ProcessedByUserCode:userCode, AccessPermissionCode:dto.DealerPermissionCode),
                            new(OrderNo:2, ProcessedByUserCode:approver, AccessPermissionCode:"A")
                        }
                    ), ct);
                // 7) Send process
                var sent = await _vnpt.SendProcessAsync(token, updated.Id, ct);


                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Đã tạo PDF, khởi tạo quy trình ký và gửi cho DealerManager.",
                    Result = new { vnptDocumentId = sent.Id, vnptNo = sent.No, vnptStatus = sent.Status.Description }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO { IsSuccess = false, StatusCode = 500, Message = $"Lỗi gửi HĐ đại lý: {ex.Message}" };
            }
        }

        public async Task<ResponseDTO> CreateDocumentAsync(VnptCreateDocReq model, Stream pdf, string fileName, CancellationToken ct)
        {
                try
                {
                    var token = await GetAccessTokenAsync(ct);
                    var created = await _vnpt.CreateDocumentAsync(token, model, pdf, fileName, ct);
                    return new ResponseDTO { IsSuccess = true, StatusCode = 201, Message = "Create document successfully", Result = created };
                }
                catch (Exception ex)
                {
                    return new ResponseDTO { IsSuccess = false, StatusCode = 500, Message = ex.Message };
                }
        }

        public async Task<ResponseDTO> UpdateProcessAsync(string token, VnptUpdateProcessReq req, CancellationToken ct)
        {
            try
            {
                var updated = await _vnpt.UpdateProcessAsync(token, req, ct);

                return new ResponseDTO { IsSuccess = true, StatusCode = 200, Message = "Update process successfully", Result = updated };
            }
            catch (Exception ex)
            {
                return new ResponseDTO { IsSuccess = false, StatusCode = 500, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO> SendProcessAsync(string token, string documentId, CancellationToken ct)
        {
            try
            {
                var sent = await _vnpt.SendProcessAsync(token, documentId, ct);
                return new ResponseDTO { IsSuccess = true, StatusCode = 200, Message = "Send process successfully", Result = sent };
            }
            catch (Exception ex)
            {
                return new ResponseDTO { IsSuccess = false, StatusCode = 500, Message = ex.Message };
            }
        }

        public async Task<List<ResponseDTO>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users, CancellationToken ct)
        {
            var list = new List<ResponseDTO>();
            try
            {
                var result = await _vnpt.CreateOrUpdateUsersAsync(token, users, ct);
                list.Add(new ResponseDTO { IsSuccess = true, StatusCode = 200, Message = "Upsert user(s) successfully", Result = result });
            }
            catch (Exception ex)
            {
                list.Add(new ResponseDTO { IsSuccess = false, StatusCode = 500, Message = ex.Message });
            }
            return list;
        }

        public Task<byte[]> DownloadAsync(string url, CancellationToken ct)
          =>  _vnpt.DownloadAsync(url, ct);
        
    }
}

