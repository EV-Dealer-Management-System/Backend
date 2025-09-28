using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IVnptEContractClient
    {
        Task<VnptResult<VnptDocumentDto>> CreateDocumentAsync(string token, CreateDocumentDTO createDocumentDTO);
        Task<VnptResult<VnptDocumentDto>> UpdateProcessAsync(string token, VnptUpdateProcessDTO processDTO);
        Task<VnptResult<VnptDocumentDto>> SendProcessAsync(string token, string documentId);
        Task<VnptResult<List<VnptUserDto>>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users);
        Task<VnptResult<ProcessRespone>> SignProcess(string token, VnptProcessDTO vnptProcessDTO);
        Task<byte[]> DownloadAsync(string url);
    }
}
