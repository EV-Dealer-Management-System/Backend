using Microsoft.Extensions.Configuration;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class SmartCAService : ISmartCAService
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        public SmartCAService(IConfiguration cfg, HttpClient http)
        {
            _cfg = cfg;
            _http = http;
        }
        public Task<SmartCATransactionCreated> CreateTransactionAsync(SmartCACreateTxnInput input, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<SmartCASignResult> GetTransactionResultAsync(string transactionId, CancellationToken token = default)
        {
            throw new NotImplementedException();
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
    }
}
