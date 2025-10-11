using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IWarehouseService
    {
        Task<ResponseDTO> CreateWarehouseAsync(CreateWarehouseDTO createWarehouseDTO);
        Task<ResponseDTO> GetAllWarehousesAsync();
        Task<ResponseDTO> GetWarehouseByIdAsync(Guid warehouseId);

    }
}
