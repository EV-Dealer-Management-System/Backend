using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EVTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IEVTemplateService
    {
        Task<ResponseDTO> CreateEVTemplateAsync(CreateEVTemplateDTO createEVTemplateDTO);
        Task<ResponseDTO> GetVehicleTemplateByIdAsync(Guid EVTemplateId);
        Task<ResponseDTO> GetAllVehicleTemplateAsync();
        Task<ResponseDTO> UpdateEVTemplateAsync(Guid EVTemplateId, UpdateEVTemplateDTO updateEVTemplateDTO);
        Task<ResponseDTO> DeleteEVTemplateAsync(Guid EVTemplateId);
        Task<ResponseDTO> GetTemplatesByVersionAndColorAsync(Guid versionId,Guid colorId);
    }
}
