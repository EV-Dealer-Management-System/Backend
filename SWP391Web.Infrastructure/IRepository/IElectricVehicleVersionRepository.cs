using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IElectricVehicleVersionRepository : IRepository<ElectricVehicleVersion>
    {
        Task<bool> IsVersionExistsById(Guid versionId);
        Task<bool> IsVersionExistsByName(string versionName);
        Task<ElectricVehicleVersion?> GetByIdsAsync(Guid versionId);
        Task<ElectricVehicleVersion?> GetByNameAsync(string versionName);

    }
}
