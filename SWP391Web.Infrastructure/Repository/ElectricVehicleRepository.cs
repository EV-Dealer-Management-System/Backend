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

        public async Task<List<ElectricVehicle>> GetAllVehicleWithDetailAsync()
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Version)
                    .ThenInclude(v => v.Model)
                .Include(ev => ev.Color)
                .ToListAsync();
        }

        public async Task<int> GetAvailableQuantityByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Where(ev =>ev.Version.ModelId == modelId
                            && ev.VersionId == versionId
                            && ev.ColorId == colorId
                            && ev.Status == StatusVehicle.Available
                            && ev.Warehouse.EVCInventoryId != null)
                .CountAsync();
        }

        public async Task<List<ElectricVehicle>> GetAvailableVehicleByModelIdAsync(Guid modelId)
        {
            return await _context.ElectricVehicles
                .Where(ev => ev.Version.ModelId == modelId
                     && ev.Status == StatusVehicle.Available
                     && ev.Warehouse.EVCInventoryId != null)
                .Include(ev => ev.Version)
                .Include(ev => ev.Color)
                .Include(ev => ev.Warehouse)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetAvailableVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Where(ev => ev.Version.ModelId == modelId
                             && ev.VersionId == versionId
                             && ev.ColorId == colorId
                             && ev.Status == StatusVehicle.Available
                             && ev.Warehouse.EVCInventoryId != null)
                .OrderBy(ev => ev.ImportDate)
                .ToListAsync();
        }

        public Task<ElectricVehicle?> GetByIdsAsync(Guid vehicleId)
        {
            throw new NotImplementedException();
        }

        public async Task<ElectricVehicle?> GetByVINAsync(string vin)
        {
            return await _context.ElectricVehicles
                .FirstOrDefaultAsync(v => v.VIN == vin);
        }

        public async Task<List<ElectricVehicle>> GetDealerInventoryAsync(Guid dealerId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Version)
                    .ThenInclude(v => v.Model)
                .Include(ev => ev.Color)
                .Include(ev => ev.Warehouse)
                .Where(ev => ev.Warehouse.DealerId == dealerId)
                .ToListAsync();
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
