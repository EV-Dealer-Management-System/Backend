using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IElectricVehicleModelService
    {
        Task<ResponseDTO> GetAllModelsAsync();
        Task<ResponseDTO> GetModelByIdAsync(Guid modelId);
        Task<ResponseDTO> CreateModelAsync(CreateElectricVehicleModelDTO createElectricVehicleModelDTO);
        Task<ResponseDTO> UpdateModelAsync(Guid modelId, UpdateElectricVehicleModelDTO updateElectricVehicleModelDTO);
        Task<ResponseDTO> DeleteModelAsync(Guid modelId);
        Task<ResponseDTO> GetModelByNameAsync(string modelName);
    }
}
