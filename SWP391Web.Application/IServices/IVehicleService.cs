using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IVehicleService
    {
        Task<ResponseDTO> GetAllVehicles(
            string? filterOn , 
            string? filterQuery , 
            string? sortBy , 
            bool? isAscending
            );
        Task<ResponseDTO> GetVehicleByIdAsync(Guid id);
        Task<ResponseDTO> CreateVehicleAsync(CreateVehicleDTO vehicleCreateDTO);
        Task<ResponseDTO> UpdateVehicleAsync(Guid id, UpdateVehicleDTO vehicleUpdateDTO);
        Task<ResponseDTO> DeleteVehicleAsync(Guid id);
        Task<ResponseDTO> GetVehicleByName(string name); 
    }
}
