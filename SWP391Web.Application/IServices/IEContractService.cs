using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Application.IServices
{
    public interface IEContractService
    {
        Task<string> GetAccessTokenAsync(CancellationToken token = default);


        // Low-level wrappers (optional but exposed here for completeness)
        Task<ResponseDTO> CreateDocumentAsync(VnptCreateDocReq model, Stream pdf, string fileName, CancellationToken ct);
        Task<ResponseDTO> UpdateProcessAsync(string token, VnptUpdateProcessReq req, CancellationToken ct);
        Task<ResponseDTO> SendProcessAsync(string token, string documentId, CancellationToken ct);
        Task<List<ResponseDTO>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users, CancellationToken ct);
        Task<byte[]> DownloadAsync(string url, CancellationToken ct);


        // Orchestrator: render PDF + push to VNPT + send
        Task<ResponseDTO> CreateAndSendAsync(CreateDealerContractDto dto, CancellationToken ct);
    }
}
