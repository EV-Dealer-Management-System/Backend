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
                .Include(ev => ev.ElectricVehicleTemplate)
                .ThenInclude(et => et.Version)
                .ThenInclude(et => et.Model)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Where(ev => ev.Status == StatusVehicle.AtDealer)
                .ToListAsync();
        }

        public async Task<int> GetAvailableQuantityByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                            && ev.ElectricVehicleTemplate.VersionId == versionId
                            && ev.ElectricVehicleTemplate.ColorId == colorId
                            && ev.Status == StatusVehicle.Available
                            && ev.Warehouse.EVCInventoryId != null)
                .CountAsync();
        }

        public Task<int> GetAvailableQuantityByVersionColorAsync(Guid versionId, Guid colorId)
        {
            return _context.ElectricVehicles
                .Where(ev => ev.VersionId == versionId
                             && ev.ColorId == colorId
                             && ev.Status == StatusVehicle.Available
                             && ev.Warehouse.EVCInventoryId != null)
                .CountAsync();
        }
        // Count vehicle in dealer 's inventory
        public async Task<int> GetAvailableVehicleAsync(Guid dealerId , Guid versionId , Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.Warehouse.DealerId == dealerId
                        && ev.ElectricVehicleTemplate.VersionId == versionId
                        && ev.ElectricVehicleTemplate.ColorId == colorId
                        && ev.Status == StatusVehicle.AtDealer)
                .CountAsync();
        }

        public async Task<List<ElectricVehicle>> GetAvailableVehicleByDealerAsync(Guid dealerId , Guid versionId , Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .ThenInclude(et => et.Version)
                .ThenInclude(v => v.Model)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Include(ev => ev.Warehouse)
                .Where( ev => ev.Warehouse.DealerId == dealerId
                        && ev.ElectricVehicleTemplate.VersionId == versionId
                        && ev.ElectricVehicleTemplate.ColorId == colorId
                        && ev.Status == StatusVehicle.AtDealer)
                .OrderBy(ev => ev.ImportDate)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetAvailableVehicleByModelIdAsync(Guid modelId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                     && ev.Status == StatusVehicle.Available
                     && ev.Warehouse.EVCInventoryId != null)
                .Include(ev => ev.ElectricVehicleTemplate.Version)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Include(ev => ev.Warehouse)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetAvailableVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                             && ev.ElectricVehicleTemplate.VersionId == versionId
                             && ev.ElectricVehicleTemplate.ColorId == colorId
                             && ev.Status == StatusVehicle.Available
                             && ev.WarehouseId != null
                             && ev.Warehouse.WarehouseType == WarehouseType.EVInventory)
                .OrderBy(ev => ev.ImportDate)
                .ToListAsync();
        }

        public async Task<ElectricVehicle?> GetByIdsAsync(Guid vehicleId)
        {
            return await _context.ElectricVehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public  async Task<ElectricVehicle?> GetByVersionColorAndWarehouseAsync(Guid versionId, Guid colorId, Guid warehouseId)
        {
            return await _context.ElectricVehicles
                .Include(v => v.Warehouse)
                .Include(v => v.ElectricVehicleTemplate)
                .FirstOrDefaultAsync(v => v.ElectricVehicleTemplate.VersionId == versionId
                                       && v.ElectricVehicleTemplate.ColorId == colorId
                                       && v.Warehouse.Id == warehouseId);
        }

        public async Task<ElectricVehicle?> GetByVINAsync(string vin)
        {
            return await _context.ElectricVehicles
                .FirstOrDefaultAsync(v => v.VIN == vin);
        }

        public async Task<List<ElectricVehicle>> GetDealerInventoryAsync(Guid dealerId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.ElectricVehicleTemplate)
                .Include(ev => ev.ElectricVehicleTemplate.Version)
                .ThenInclude(v => v.Model)
                .Include(ev => ev.ElectricVehicleTemplate.Color)
                .Include(ev => ev.Warehouse)
                .Where(ev => ev.Warehouse.DealerId == dealerId
                            && ev.Status == StatusVehicle.AtDealer)
                .ToListAsync();
        }

        public async Task<List<ElectricVehicle>> GetPendingVehicleByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicles
                .Include(ev => ev.Warehouse)
                .Include(ev => ev.ElectricVehicleTemplate)
                .Where(ev => ev.ElectricVehicleTemplate.Version.ModelId == modelId
                             && ev.ElectricVehicleTemplate.VersionId == versionId
                             && ev.ElectricVehicleTemplate.ColorId == colorId
                             && ev.Status == StatusVehicle.Pending
                             && ev.WarehouseId != null
                             && ev.Warehouse.WarehouseType == WarehouseType.EVInventory)
                .OrderBy(ev => ev.ImportDate)
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
