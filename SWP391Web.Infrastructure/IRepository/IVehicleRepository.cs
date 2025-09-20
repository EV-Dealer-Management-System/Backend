using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {
        Task<Vehicle?> GetByNameAsync(string name);
        Task<Vehicle?> GetByModelAsync(string model);
        Task<Vehicle> GetByVersionAsync(string version);
        Task<Vehicle?> GetByIdAsync(Guid id);
        Task<Vehicle?> GetByIdAndNameAsync(Guid id, string name);
        Task<bool> IsExistByName(string name);
        Task<bool> IsExistByNameAndId(string name, Guid id);

    }
}
