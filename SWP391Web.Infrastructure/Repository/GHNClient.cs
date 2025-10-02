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
            if (payload is not null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            }

            return request;
        }

        public Task<GhnResult<List<GhnDistrict>>> GetDistrictsAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<GhnResult<List<GhnProvince>>> GetProvincesAsync(CancellationToken ct = default)
        {
            var request = JsonRequest(HttpMethod.Get, $"/province");
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                return GhnResult<List<GhnProvince>>.Fail($"GHN API error: {response.StatusCode} - {json}");
            }
            return JsonConvert.DeserializeObject<GhnResult<List<GhnProvince>>>(json) ?? GhnResult<List<GhnProvince>>.Fail("Deserialize error");
        }

        public Task<GhnResult<List<GhnWard>>> GetWardsAsync(int districtId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        private HttpRequestMessage AbsoluteGet(string absoluteUrl)
        {
            return new HttpRequestMessage(HttpMethod.Get, absoluteUrl);
        }

        public Task<List<ProvincesOpenGetProvinceResponse>> ProvincesOpenGetProvinceResponse(CancellationToken ct = default)
        {
            var request = AbsoluteGet($"https://provinces.open-api.vn/api/v2/p/");
            var response = _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).Result;

            var json = response.Content.ReadAsStringAsync(ct).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Provinces Open API error: {response.StatusCode} - {json}");
            }
            return Task.FromResult(JsonConvert.DeserializeObject<List<ProvincesOpenGetProvinceResponse>>(json) ?? throw new Exception("Deserialize error"));
        }

        public Task<ProvincesOpenGetWardResponse> ProvincesOpenGetWardResponse(string provinceCode, CancellationToken ct = default)
        {
            var request = AbsoluteGet($"https://provinces.open-api.vn/api/v2/p/{provinceCode}?depth=2");
            var response = _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).Result;
            var json = response.Content.ReadAsStringAsync(ct).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Provinces Open API error: {response.StatusCode} - {json}");
            }
            return Task.FromResult(JsonConvert.DeserializeObject<ProvincesOpenGetWardResponse>(json) ?? throw new Exception("Deserialize error"));
        }
    }
}
