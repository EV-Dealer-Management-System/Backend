using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicleVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IElectricVehicleVersionService
    {
        Task<ResponseDTO> GetAllVersionsAsync();
        Task<ResponseDTO> GetVersionByIdAsync(Guid versionId);
        Task<ResponseDTO> GetVersionByNameAsync(string versionName);
        Task<ResponseDTO> CreateVersionAsync(CreateElectricVehicleVersionDTO createElectricVehicleVersionDTO);
        Task<ResponseDTO> UpdateVersionAsync(Guid versionId, UpdateElectricVehicleVersionDTO updateElectricVehicleVersionDTO);
        Task<ResponseDTO> DeleteVersionAsync(Guid versionId);
        Task<ResponseDTO> GetAllAvailableVersionsByModelIdAsync(Guid modelId);

    }
}
