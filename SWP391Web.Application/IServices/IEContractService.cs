using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Application.IServices
{
    public interface IEContractService
    {
        Task<string> GetAccessTokenAsync();
        Task<byte[]> DownloadAsync(string url);

        Task<ResponseDTO> SignProcess(VnptProcessDTO vnptProcessDTO);

        // Orchestrator: render PDF + push to VNPT + send
        Task<ResponseDTO> CreateAndSendAsync(CreateDealerDTO createDealerDTO);
    }
}
