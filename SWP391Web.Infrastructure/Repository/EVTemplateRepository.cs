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
    public class EVTemplateRepository : Repository<ElectricVehicleTemplate>, IEVTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        public EVTemplateRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<ElectricVehicleTemplate?> GetByIdAsync(Guid EVTemplateId)
        {
            return await _context.ElectricVehicleTemplates.FirstOrDefaultAsync(evt => evt.Id == EVTemplateId);
        }

        public async Task<ElectricVehicleTemplate?> GetByVersionColorAndWarehouseAsync(Guid versionId, Guid colorId, Guid warehouseId)
        {
            return await _context.ElectricVehicleTemplates
                .Include(evt => evt.ElectricVehicles)
                    .ThenInclude(ev => ev.Warehouse)
                .FirstOrDefaultAsync(t =>t.VersionId == versionId 
                                    && t.ColorId == colorId 
                                    && t.ElectricVehicles.Any(ev => ev.WarehouseId == warehouseId 
                                    && ev.Warehouse.WarehouseType == WarehouseType.Dealer));
        }

        public async Task<List<ElectricVehicleTemplate>> GetTemplatesByVersionAndColorAsync(Guid versionId, Guid colorId)
        {
            return await _context.ElectricVehicleTemplates
                .Include(evt => evt.Version)
                    .ThenInclude(v => v.Model)
                .Include(evt => evt.Color)
                .Where(evt => evt.VersionId == versionId && evt.ColorId == colorId)
                .ToListAsync();
        }

        public async Task<bool>? IsEVTemplateExistsById(Guid EVTemplateId)
        {
            return await _context.ElectricVehicleTemplates.AnyAsync(evt => evt.Id == EVTemplateId);
        }
    }
}
