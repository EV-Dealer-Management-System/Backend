using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IVehicleColorRepository : IRepository<VehicleColor>
    {
        Task<VehicleColor?> GetByIdAsync(Guid id);
        Task<VehicleColor?> GetByNameAsync(string colorName);
        Task<bool> IsExistByColorName(string colorName);
        
    }
}
