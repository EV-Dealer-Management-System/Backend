using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IElectricVehicleModelRepository : IRepository<ElectricVehicleModel>
    {
        Task<ElectricVehicleModel?> GetByNameAsync(string modelName);
        Task<ElectricVehicleModel?> GetByIdsAsync(Guid modelId);
        Task<List<ElectricVehicleModel>> GetAllWithVersionAsync();
        Task<bool> IsModelExistsById(Guid modelId);
        Task<bool> IsModelExistsByName(string modelName);
    }
}
