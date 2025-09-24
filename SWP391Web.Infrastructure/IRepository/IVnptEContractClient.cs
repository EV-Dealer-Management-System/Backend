using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IVnptEContractClient
    {
        Task<VnptCreateDocResp> CreateDocumentAsync(string token, VnptCreateDocReq req, Stream pdf, string fileName, CancellationToken ct);
        Task<VnptDocDto> UpdateProcessAsync(string token, VnptUpdateProcessReq req, CancellationToken ct);
        Task<VnptDocDto> SendProcessAsync(string token, string documentId, CancellationToken ct);
        Task<List<VnptUserDto>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users, CancellationToken ct);
        Task<byte[]> DownloadAsync(string url, CancellationToken ct);
    }
}
