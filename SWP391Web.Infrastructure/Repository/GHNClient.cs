using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SWP391Web.Domain.ValueObjects;
using SWP391Web.Infrastructure.IRepository;
using System.Text;

namespace SWP391Web.Infrastructure.Repository
{
    public class GHNClient : IGHNClient
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private static string? _baseUrl;
        public GHNClient(IConfiguration cfg, HttpClient http)
        {
            _cfg = cfg;
            _http = http;
            _baseUrl = _cfg["GHN:BaseUrl"];
        }

        private HttpRequestMessage JsonRequest(HttpMethod method, string path, object? payload = null)
        {
            var request = new HttpRequestMessage(method, _baseUrl + path);
            request.Headers.Add("Token", _cfg["GHN:Token"] ?? throw new ArgumentNullException("GHN:Token is not exist"));
            if(payload is not null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            }

            return request;
        }

        public async Task<GhnResult<List<GhnDistrict>>> GetDistrictsAsync(int provinceId, CancellationToken ct = default)
        {
            var request = JsonRequest(HttpMethod.Get, $"/district?province_id={provinceId}", ct);
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                return GhnResult<List<GhnDistrict>>.Fail($"GHN API error: {response.StatusCode} - {json}");
            }
            return JsonConvert.DeserializeObject<GhnResult<List<GhnDistrict>>>(json) ?? GhnResult<List<GhnDistrict>>.Fail("Deserialize error");
        }

        public async Task<GhnResult<List<GhnProvince>>> GetProvincesAsync(CancellationToken ct = default)
        {
            var request = JsonRequest(HttpMethod.Get, $"/province", ct);
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                return GhnResult<List<GhnProvince>>.Fail($"GHN API error: {response.StatusCode} - {json}");
            }
            return JsonConvert.DeserializeObject<GhnResult<List<GhnProvince>>>(json) ?? GhnResult<List<GhnProvince>>.Fail("Deserialize error");
        }

        public async Task<GhnResult<List<GhnWard>>> GetWardsAsync(int districtId, CancellationToken ct = default)
        {
            var request = JsonRequest(HttpMethod.Get, $"/ward?district_id={districtId}", ct);
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                return GhnResult<List<GhnWard>>.Fail($"GHN API error: {response.StatusCode} - {json}");
            }
            return JsonConvert.DeserializeObject<GhnResult<List<GhnWard>>>(json) ?? GhnResult<List<GhnWard>>.Fail("Deserialize error");
        }
    }
}
