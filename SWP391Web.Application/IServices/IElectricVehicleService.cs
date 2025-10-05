using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IElectricVehicleService
    {
        Task<ResponseDTO> GetAllVehiclesAsync();
        Task<ResponseDTO> GetVehicleByIdAsync(Guid vehicleId);
        Task<ResponseDTO> GetVehicleByVinAsync(string vin);
        Task<ResponseDTO> CreateVehicleAsync(CreateElecticVehicleDTO createElectricVehicleDTO);
        Task<ResponseDTO> UpdateVehicleAsync(Guid vehicleId, UpdateElectricVehicleDTO updateElectricVehicleDTO);

    }
}
