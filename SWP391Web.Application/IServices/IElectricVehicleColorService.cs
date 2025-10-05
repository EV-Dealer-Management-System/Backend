using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicleColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IElectricVehicleColorService
    {
        Task<ResponseDTO> GetAllColorsAsync();
        Task<ResponseDTO> GetColorByIdAsync(Guid colorId);
        Task<ResponseDTO> CreateColorAsync(CreateElectricVehicleColorDTO createElectricVehicleColorDTO);
        Task<ResponseDTO> UpdateColorAsync(Guid colorId, UpdateElectricVehicleColor updateElectricVehicleColor);
        Task<ResponseDTO> DeleteColorAsync(Guid colorId);
        Task<ResponseDTO> GetColorByNameAsync(string colorName);
        Task<ResponseDTO> GetColorByCodeAsync(string colorCode);
        
    }
}
