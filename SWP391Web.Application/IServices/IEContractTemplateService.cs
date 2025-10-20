using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EContractTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IEContractTemplateService
    {
        Task<ResponseDTO> CreateEContractTemplateAsync(CreateEContractTemplateDTO templateDTO, CancellationToken ct);
        Task<ResponseDTO> GetEContractTemplateByEcontractIdAsync(Guid eContractId, CancellationToken ct);
        Task<ResponseDTO> GetAll(int? pageNumber, int? pageSize, CancellationToken ct);
        Task<ResponseDTO> UpdateEcontractTemplateAsync(string code, UpdateEContractTemplateDTO templateDTO, CancellationToken ct);
    }
}
