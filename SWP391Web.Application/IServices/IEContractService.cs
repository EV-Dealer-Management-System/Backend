using EVManagementSystem.Application.DTO.EContract;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EContract;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Domain.ValueObjects;
using System.Security.Claims;

namespace SWP391Web.Application.IServices
{
    public interface IEContractService
    {
        Task<string> GetAccessTokenAsync();
        Task<ProcessLoginInfoDto> GetAccessTokenAsyncByCode(string processCode, CancellationToken ct = default);
        Task<byte[]> DownloadAsync(string url);

        Task<ResponseDTO> SignProcess(string token, VnptProcessDTO vnptProcessDTO, CancellationToken ct);
        Task<HttpResponseMessage> GetPreviewResponseAsync(string token, string? rangeHeader = null, CancellationToken ct = default);
        Task<ResponseDTO> CreateEContractAsync(ClaimsPrincipal userClaim, CreateEContractDTO createEContractDTO, CancellationToken ct);
        Task<VnptResult<VnptSmartCAResponse>> AddSmartCA(AddNewSmartCADTO addNewSmartCADTO);
        Task<VnptResult<VnptFullUserData>> GetSmartCAInformation(int userId);
        Task<VnptResult<VnptSmartCAResponse>> UpdateSmartCA(UpdateSmartDTO updateSmartDTO);
        Task<VnptResult<UpdateEContractResponse>> UpdateEContract(UpdateEContractDTO updateEContractDTO, CancellationToken ct);
        Task<ResponseDTO<EContract>> GetAllEContractList(int? pageNumber, int? pageSize, EContractStatus eContractStatus);
        Task<ResponseDTO> CreateDraftEContractAsync(ClaimsPrincipal userClaim, CreateDealerDTO createDealerDTO, CancellationToken ct);
        Task<VnptResult<VnptDocumentDto>> GetVnptEContractByIdAsync(string eContractId, CancellationToken ct);
        Task<ResponseDTO<EContract>> GetEContractByIdAsync(string eContractId, CancellationToken ct);
        Task<VnptResult<GetEContractResponse<DocumentListItemDto>>> GetAllVnptEContractList(int? pageNumber, int? pageSize, EContractStatus eContractStatus);
        Task<ResponseDTO> SnapshotEcontract(Guid EcontractId, string key, CancellationToken ct);
    }
}
