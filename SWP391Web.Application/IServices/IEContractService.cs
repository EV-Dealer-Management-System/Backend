using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EContract;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Application.IServices
{
    public interface IEContractService
    {
        Task<string> GetAccessTokenAsync();
        Task<ProcessLoginInfoDto> GetAccessTokenAsyncByCode(string processCode, CancellationToken ct = default);
        Task<byte[]> DownloadAsync(string url);

        Task<ResponseDTO> SignProcess(string token, VnptProcessDTO vnptProcessDTO);
        Task<HttpResponseMessage> GetPreviewResponseAsync(string token, string? rangeHeader = null, CancellationToken ct = default);
        Task<ResponseDTO> CreateEContractAsync(CreateDealerDTO createDealerDTO, CancellationToken ct);
        Task<VnptResult<VnptSmartCAResponse>> AddSmartCA(AddNewSmartCADTO addNewSmartCADTO);
        Task<VnptResult<VnptFullUserData>> GetSmartCAInformation(int userId);
    }
}
