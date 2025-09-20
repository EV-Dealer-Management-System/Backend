using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Repository
{
    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        public readonly ApplicationDbContext _context;
        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<Vehicle?> GetByIdAndNameAsync(Guid id, string name)
        {
            return _context.Vehicles
                .Include(c => c.VehicleId)
                .Include(c => c.Name)
                .FirstOrDefaultAsync(c => c.VehicleId == id && c.Name == name);
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id)
        {
            return await _context.Vehicles
                .Include(c => c.VehicleId)
                .FirstOrDefaultAsync(c => c.VehicleId == id);
        }

        public async Task<Vehicle?> GetByModelAsync(string model)
        {
            return await _context.Vehicles
                .Include(c => c.Model)
                .FirstOrDefaultAsync(c => c.Model == model);
        }

        public async Task<Vehicle?> GetByNameAsync(string name)
        {
            
            return await _context.Vehicles
                .Include(c => c.Name)
                .FirstOrDefaultAsync(c => c.Name == name);  
        }

        public async Task<Vehicle> GetByVersionAsync(string version)
        {
            return await _context.Vehicles
                .Include(c => c.Version)
                .FirstOrDefaultAsync(c => c.Version == version);
        }

        public async Task<bool> IsExistByName(string name)
        {
            return await _context.Vehicles.AnyAsync(v => v.Name == name);
        }

        public async Task<bool> IsExistByNameAndId(string name, Guid id)
        {
            var trimmedName = name.Trim();
            return await _context.Vehicles.AnyAsync(v => v.Name.ToUpper() == name.ToUpper() && v.VehicleId != id);
        }
    }
}
