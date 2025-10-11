using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IWarehouseRepository : IRepository<Warehouse>
    {
        Task<Warehouse?> GetWarehouseByIdAsync(Guid warehouseId);
        Task<bool> IsWareHouseExistByDealerIdAsync(Guid dealerId);
        Task<bool> IsWareHouseExistByEVCInventoryIdAsync(Guid evcInventoryId);
    }
}
