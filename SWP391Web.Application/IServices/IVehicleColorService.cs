using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.VehicleColorDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IVehicleColorService 
    {
        Task<ResponseDTO> GetAllVehicleColors();
        Task<ResponseDTO> GetVehicleColorByIdAsync(Guid id);
        Task<ResponseDTO> CreateVehicleColorAsync(CreateVehicleColorDTO vehicleColorCreateDTO);
        Task<ResponseDTO> DeleteVehicleColorAsync(Guid id);
        Task<ResponseDTO> GetVehicleColorByName(string name);
    }
}
