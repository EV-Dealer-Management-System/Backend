using Microsoft.Extensions.Configuration;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO;
using SWP391Web.Domain.ValueObjects;
using SWP391Web.Infrastructure.IRepository;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SWP391Web.Domain.Enums;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

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
            _baseUrl = _cfg["EContractClient:BaseUrl"];
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

            return await SendAsync<T>(request);
        }

        private async Task<VnptResult<T>> GetAsync<T>(string token, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _baseUrl + url);
            Bearer(request, token);

            return await SendAsync<T>(request);
        }

        public async Task<VnptResult<VnptDocumentDto>> UpdateProcessAsync(string token, VnptUpdateProcessDTO processDTO)
            => await PostAsync<VnptDocumentDto>(token, "/api/documents/update-process", processDTO);

        public async Task<VnptResult<VnptDocumentDto>> SendProcessAsync(string token, string documentId)
        => await PostAsync<VnptDocumentDto>(token, $"/api/documents/send-process/{documentId}", null);


        public async Task<byte[]> DownloadAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Download URL is required", nameof(url));

            using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<VnptResult<ProcessRespone>> SignProcess(string token, VnptProcessDTO vnptProcessDTO)
            => await PostAsync<ProcessRespone>(token, "/api/documents/process", vnptProcessDTO);


        public async Task<HttpResponseMessage> GetDownloadResponseAsync(string downloadURL, string? rangeHeader = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(downloadURL))
                throw new ArgumentException("downloadURL is required", nameof(downloadURL));

            var url = $"{downloadURL}";
            var req = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrWhiteSpace(rangeHeader))
            {
                req.Headers.TryAddWithoutValidation("Range", rangeHeader);
            }

            var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

            return res;
        }

        public async Task<VnptResult<VnptSmartCAResponse>> AddSmartCA(string token, AddNewSmartCADTO addNewSmartCADTO)
            => await PostAsync<VnptSmartCAResponse>(token, "/api/users/smart-ca/add", addNewSmartCADTO);

        public async Task<VnptResult<VnptFullUserData>> GetSmartCAInformation(string token, int userId)
            => await GetAsync<VnptFullUserData>(token, $"/api/users/{userId}");

        public async Task<VnptResult<VnptSmartCAResponse>> UpdateSmartCA(string token, UpdateSmartDTO updateSmartDTO)
            => await PostAsync<VnptSmartCAResponse>(token, "/api/users/smart-ca/update", updateSmartDTO);

        public async Task<VnptResult<UpdateEContractResponse>> UpdateEContractAsync(string token, string id, string subject, IFormFile file)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/api/documents/update");
            Bearer(httpRequest, token);

            var content = new MultipartFormDataContent
           {
               { new StringContent(id), "Id" },
                { new StringContent(subject ?? ""), "Subject" }
           };

            using var fileStream = file.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            content.Add(streamContent, "File", file.FileName);

            httpRequest.Content = content;

            return await SendAsync<UpdateEContractResponse>(httpRequest);
        }

        public async Task<VnptResult<UpdateEContractResponse>> UpdateEContract(string token, string id, string subject, IFormFile file)
        {
            var result = await UpdateEContractAsync(token, id, subject, file);
            //var pdfBytes = await GetPdfBytesFromDownloadUrlAsync(result.Data?.DownloadUrl, token);

            //result.Data!.FileBytes = pdfBytes;
            return result;
        }

        public async Task<VnptResult<GetEContractResponse<DocumentListItemDto>>> GetEContractList(string token, int? pageNumber, int? pageSize, EContractStatus eContractStatus)
            => await GetAsync<GetEContractResponse<DocumentListItemDto>>(token, $"/api/documents?page={pageNumber ?? 1}&pageSize={pageSize ?? 10}&status={(int)eContractStatus}");

        public async Task<VnptResult<VnptDocumentDto>> GetEContractByIdAsync(string token, string eContractId)
            => await GetAsync<VnptDocumentDto>(token, $"/api/documents/{eContractId}");
        
    }
}
