using Microsoft.Extensions.Configuration;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO;
using SWP391Web.Domain.ValueObjects;
using SWP391Web.Infrastructure.IRepository;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace SWP391Web.Infrastructure.Repository
{
    public class VnptEContractClient : IVnptEContractClient
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private static string? _baseUrl;

        public VnptEContractClient(IConfiguration cfg, HttpClient http)
        {
            _cfg = cfg;
            _http = http;
            _baseUrl = _cfg["SmartCA:BaseUrl"];
        }

        private static void Bearer(HttpRequestMessage request, string token)
            => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        public async Task<VnptResult<VnptDocumentDto>> CreateDocumentAsync(string token, CreateDocumentDTO createDocumentDTO)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/api/documents/create");
            Bearer(httpRequest, token);
            var content = new MultipartFormDataContent
            {
                {new StringContent(createDocumentDTO.No!), "No" },
                {new StringContent(createDocumentDTO.Subject!), "Subject" },
                {new StringContent(createDocumentDTO.Description ?? ""), "Description" },
                {new StringContent(createDocumentDTO.TypeId.ToString()), "TypeId" },
                {new StringContent(createDocumentDTO.DepartmentId.ToString()), "DepartmentId" }
            };

            if (!string.IsNullOrWhiteSpace(createDocumentDTO.FileInfo.FilePath))
            {
                await using var fileStream = File.OpenRead(createDocumentDTO.FileInfo.FilePath);
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content.Add(streamContent, "File", createDocumentDTO.FileInfo.FileName ?? Path.GetFileName(createDocumentDTO.FileInfo.FilePath));
            }
            else
            {
                if (createDocumentDTO.FileInfo.File is null || createDocumentDTO.FileInfo.File.Length == 0)
                {
                    throw new InvalidOperationException("File bytes is empty. Privide FileInfo.File pr FileInfo.FilePath.");
                }

                var byteAC = new ByteArrayContent(createDocumentDTO.FileInfo.File);
                byteAC.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content.Add(byteAC, "File", createDocumentDTO.FileInfo.FileName ?? $"EContract-{DateTime.UtcNow:ssmmHHddMMyyyy}");
            }

            httpRequest.Content = content;
            return await SendAsync<VnptDocumentDto>(httpRequest);
        }

        public async Task<VnptResult<List<VnptUserDto>>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/api/users/create-or-update");
            Bearer(httpRequest, token);

            var jsonPayload = JsonConvert.SerializeObject(users);
            httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            return await SendAsync<List<VnptUserDto>>(httpRequest);
        }

        //public async Task<byte[]> DownloadAsync(string url, CancellationToken ct)
        //{
        //    using var res = await _http.GetAsync(url, ct);
        //    res.EnsureSuccessStatusCode();
        //    return await res.Content.ReadAsByteArrayAsync(ct);
        //}

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
            Bearer(request, token);

            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new VnptResult<T>($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n{request.Method} {request.RequestUri}\n{content}");
            }

            return JsonConvert.DeserializeObject<VnptResult<T>>(content) ?? new("Fail");
        }

        public async Task<VnptResult<VnptDocumentDto>> UpdateProcessAsync(string token, VnptUpdateProcessDTO processDTO)
            => await PostAsync<VnptDocumentDto>(token, "/api/documents/update-process", processDTO);

        public async Task<VnptResult<VnptDocumentDto>> SendProcessAsync(string token, string documentId)
        => await PostAsync<VnptDocumentDto>(token, $"/api/documents/send-process/{documentId}", null);


        public Task<byte[]> DownloadAsync(string url)
        {
            throw new NotImplementedException();
        }

        public async Task<VnptResult<ProcessRespone>> SignProcess(string token, VnptProcessDTO vnptProcessDTO)
        {
            var jsonPayload = JsonConvert.SerializeObject(vnptProcessDTO, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return await PostAsync<ProcessRespone>(token, "/api/documents/process", vnptProcessDTO);
        }

        //public async Task<VnptDocumentDto> UpdateProcessAsync(string token, VnptUpdateProcessReq reqMedel, CancellationToken ct)
        //{
        //    var req = JsonReq(HttpMethod.Post, $"{_cfg["SmartCA:BaseUrl"]}/api/documents/update-process", reqMedel);
        //    Bearer(req, token);

        //    using var res = await _http.SendAsync(req, ct);
        //    return await ReadOrThrowAsync<VnptDocumentDto>(res, ct);
        //}

        //private async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage res, CancellationToken ct)
        //{
        //    var body = await res.Content.ReadAsStringAsync(ct);
        //    if (!res.IsSuccessStatusCode)
        //        throw new HttpRequestException($"HTTP {(int)res.StatusCode} {res.ReasonPhrase}\n{res.RequestMessage?.Method} {res.RequestMessage?.RequestUri}\n{body}");

        //    var env = Newtonsoft.Json.JsonSerializer.Deserialize<VnptEnvelope<T>>(body, _jon);
        //    if (env is null || !env.Success || env.Data is null)
        //        throw new HttpRequestException($"VNPT returned success=false or data=null\n{res.RequestMessage?.Method} {res.RequestMessage?.RequestUri}\n{body}");

        //    return env.Data;
        //}
    }
}
