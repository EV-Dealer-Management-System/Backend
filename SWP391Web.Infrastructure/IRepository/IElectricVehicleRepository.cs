using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IElectricVehicleRepository : IRepository<ElectricVehicle>
    {
        Task<bool> IsVehicleExistsById(Guid vehicleId);
        Task<bool> IsVehicleExistsByVIN(string vin);
        Task<ElectricVehicle?> GetByIdsAsync(Guid vehicleId);
        Task<ElectricVehicle?> GetByVINAsync(string vin);
        Task<List<ElectricVehicle>> GetAvailableVehicleByModelIdAsync(Guid modelId);
        Task<int> GetAvailableQuantityByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId);
        Task<int> GetAvailableQuantityByVersionColorAsync(Guid versionId, Guid colorId);
        Task<List<ElectricVehicle>> GetAvailableVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId);
        Task<List<ElectricVehicle>> GetDealerInventoryAsync(Guid dealerId);
        Task<List<ElectricVehicle>> GetAllVehicleWithDetailAsync();
        Task<List<ElectricVehicle>> GetAvailableVehicleByDealerAsync(Guid dealerId, Guid versionId, Guid colorId);
        Task<int> GetAvailableVehicleAsync(Guid dealerId, Guid versionId, Guid colorId);
        Task<ElectricVehicle?> GetByVersionColorAndWarehouseAsync(Guid versionId, Guid colorId, Guid warehouseId);
        Task<List<ElectricVehicle>> GetPendingVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId);
        Task<List<ElectricVehicle>> GetVehicleByQuantityWithOldestImportDateForDealerAsync(Guid versionId, Guid colorId, Guid warehouseId, int quantity);
    }
}
