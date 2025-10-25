using Microsoft.AspNetCore.Http;
using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.Enums;
using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Infrastructure.IClient
{
    public interface IVnptEContractClient
    {
        Task<VnptResult<VnptDocumentDto>> CreateDocumentAsync(string token, CreateDocumentDTO createDocumentDTO);
        Task<VnptResult<VnptDocumentDto>> UpdateProcessAsync(string token, VnptUpdateProcessDTO processDTO);
        Task<VnptResult<VnptDocumentDto>> SendProcessAsync(string token, string documentId);
        Task<VnptResult<List<VnptUserDto>>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users);
        Task<VnptResult<ProcessRespone>> SignProcess(string token, VnptProcessDTO vnptProcessDTO);
        Task<HttpResponseMessage> GetDownloadResponseAsync(string downloadUrl, string? rangeHeader, CancellationToken ct = default);
        Task<VnptResult<VnptSmartCAResponse>> AddSmartCA(string token, AddNewSmartCADTO addNewSmartCADTO);
        Task<VnptResult<VnptFullUserData>> GetSmartCAInformation(string token, int userId);
        Task<VnptResult<VnptSmartCAResponse>> UpdateSmartCA(string token, UpdateSmartDTO updateSmartDTO);
        Task<VnptResult<UpdateEContractResponse>> UpdateEContract(string token, string id, string subject, IFormFile file);
        Task<VnptResult<GetEContractResponse<DocumentListItemDto>>> GetEContractList(string token, int? pageNumber, int? pageSize, EContractStatus eContractStatus);
        Task<VnptResult<VnptDocumentDto>> GetEContractByIdAsync(string token, string eContractId);
        Task<byte[]> DownloadAsync(string url);
    }
}
