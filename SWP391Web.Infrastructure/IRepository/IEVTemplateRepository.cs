using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IEVTemplateRepository :IRepository<ElectricVehicleTemplate>
    {
        Task<ElectricVehicleTemplate?> GetByIdAsync(Guid EVTemplateId);
        Task<bool> IsEVTemplateExistsById(Guid EVTemplateId);
        Task<List<ElectricVehicleTemplate>> GetTemplatesByVersionAndColorAsync(Guid versionId, Guid colorId);
        Task<ElectricVehicleTemplate?> GetByVersionColorAndWarehouseAsync(Guid versionId, Guid colorId, Guid warehouseId);
    }
}
