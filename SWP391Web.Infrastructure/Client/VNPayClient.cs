using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SWP391Web.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SWP391Web.Infrastructure.Client
{
    internal class VNPayClient
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private static string? _baseUrl;
        public VNPayClient(IConfiguration cfg, HttpClient http)
        {
            _cfg = cfg;
            _http = http;
            _baseUrl = _cfg["VNPay:BaseUrl"];
        }
        private async Task<VnptResult<T>> SendAsync<T>(HttpRequestMessage httpRequest)
        {
            var response = await _http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new VnptResult<T>($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}\n{content}");
            }

            return JsonConvert.DeserializeObject<VnptResult<T>>(content) ?? new("Fail");
        }

        private async Task<VnptResult<T>> PostAsync<T>(string token, string url, object? payload)
        {
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + url)
            {
                Content = httpContent
            };

            return await SendAsync<T>(request);
        }

        private async Task<VnptResult<T>> GetAsync<T>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _baseUrl + url);

            return await SendAsync<T>(request);
        }

        public async Task<object> CreatePaymentUrl(string version, string command, string tmnCode, int amount, string bankCode, string createDate, string currCode, string ipAddr, string locale
            string orderInfo, int orderType)
        {
            return await GetAsync<object>($"/{}");
        }
    }
}
