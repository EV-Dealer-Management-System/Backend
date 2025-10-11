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
        Task<List<Guid>> GetAvailableVersionIdsByModelIdAsync(Guid modelId);
    }
}
