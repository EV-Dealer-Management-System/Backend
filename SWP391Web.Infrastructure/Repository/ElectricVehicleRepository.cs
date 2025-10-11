using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Repository
{
    public class ElectricVehicleRepository : Repository<ElectricVehicle>, IElectricVehicleRepository
    {
        public readonly ApplicationDbContext _context;
        public ElectricVehicleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Guid>> GetAvailableVersionIdsByModelIdAsync(Guid modelId)
        {
            return await _context.ElectricVehicles
                .Where(ev => ev.Status == StatusVehicle.Available 
                && _context.EVCInventories.Any(w => w.Id == ev.WarehouseId)) // Ensure the vehicle is in a warehouse
                .Select(ev => ev.VersionId)
                .Distinct()
                .ToListAsync();

        }

        public async Task<ElectricVehicle?> GetByIdsAsync(Guid vehicleId)
        {
            return await _context.ElectricVehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public async Task<ElectricVehicle?> GetByVINAsync(string vin)
        {
            return await _context.ElectricVehicles
                .FirstOrDefaultAsync(v => v.VIN == vin);
        }

        public async Task<bool> IsVehicleExistsById(Guid vehicleId)
        {
            return await _context.ElectricVehicles
                .AnyAsync(v => v.Id == vehicleId);
        }

        public async Task<bool> IsVehicleExistsByVIN(string vin)
        {
            return await _context.ElectricVehicles
                .AnyAsync(v => v.VIN == vin);
        }
    }
}
