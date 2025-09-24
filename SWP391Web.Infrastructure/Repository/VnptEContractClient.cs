using Microsoft.Extensions.Configuration;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.ValueObjects;
using SWP391Web.Infrastructure.IRepository;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SWP391Web.Infrastructure.Repository
{
    public class VnptEContractClient : IVnptEContractClient
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jon = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public VnptEContractClient(IConfiguration cfg, HttpClient http)
        {
            _cfg = cfg;
            _http = http;
        }

        private static HttpRequestMessage JsonReq(HttpMethod m, string url, object? payload = null)
        => new HttpRequestMessage(m, url)
        { Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json") };

        private static void Bearer(HttpRequestMessage req, string token)
            => req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        public async Task<VnptCreateDocResp> CreateDocumentAsync(string token, VnptCreateDocReq model, Stream pdf, string fileName, CancellationToken ct)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model.No), "no");
            form.Add(new StringContent(model.Subject), "subject");
            if (!string.IsNullOrWhiteSpace(model.Description))
                form.Add(new StringContent(model.Description), "description");
            form.Add(new StringContent(model.TypeId.ToString()), "typeId");
            form.Add(new StringContent(model.DepartmentId.ToString()), "departmenId");

            var fileContent = new StreamContent(pdf);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(fileContent, "file", fileName);

            var req = new HttpRequestMessage(HttpMethod.Post, $"{_cfg["SmartCA:BaseUrl"]}/api/documents/create")
            {
                Content = form
            };
            Bearer(req, token);

            using var res = await _http.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync(ct);

            var result = JsonSerializer.Deserialize<VnptEnvelope<VnptCreateDocResp>>(body, _jon)!.Data;

            return result;
        }

        public async Task<List<VnptUserDto>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users, CancellationToken ct)
        {
            var req = JsonReq(HttpMethod.Post, $"{_cfg["SmartCA:BaseUrl"]}//api/users/create-or-update", users);
            Bearer(req, token);

            using var res = _http.Send(req, ct);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<VnptEnvelope<List<VnptUserDto>>>(body, _jon)!.Data;
        }

        public async Task<byte[]> DownloadAsync(string url, CancellationToken ct)
        {
            using var res = await _http.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsByteArrayAsync(ct);
        }

        public async Task<VnptDocDto> SendProcessAsync(string token, string documentId, CancellationToken ct)
        {
            var req = JsonReq(HttpMethod.Post, $"{_cfg["SmartCA:BaseUrl"]}/api/documents/send-process/{documentId}");
            Bearer(req, token);

            using var res = _http.Send(req, ct);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<VnptEnvelope<VnptDocDto>>(body, _jon)!.Data;
        }

        public async Task<VnptDocDto> UpdateProcessAsync(string token, VnptUpdateProcessReq reqMedel, CancellationToken ct)
        {
            var req = JsonReq(HttpMethod.Post, $"{_cfg["SmartCA:BaseUrl"]}/api/documents/update-process", reqMedel);
            Bearer(req, token);

            using var res = _http.Send(req, ct);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<VnptEnvelope<VnptDocDto>>(body, _jon)!.Data;
        }

    }
}
