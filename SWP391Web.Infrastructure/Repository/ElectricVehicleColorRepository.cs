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
    public class ElectricVehicleColorRepository : Repository<ElectricVehicleColor>, IElectricVehicleColorRepository
    {
        public readonly ApplicationDbContext _context;
        public ElectricVehicleColorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ElectricVehicleColor?> GetByCodeAsync(string colorCode)
        {
            return await _context.ElectricVehicleColors
                .FirstOrDefaultAsync(c => c.ColorCode == colorCode);
        }

        public async Task<ElectricVehicleColor?> GetByIdsAsync(Guid colorId)
        {
            return await _context.ElectricVehicleColors
                .FirstOrDefaultAsync(c => c.Id == colorId);
        }

        public Task<ElectricVehicleColor?> GetByNameAsync(string colorName)
        {
            return _context.ElectricVehicleColors
                .FirstOrDefaultAsync(c => c.ColorName == colorName);
        }

        public async Task<bool> IsColorExistsByCode(string colorCode)
        {
            return await _context.ElectricVehicleColors
                .AnyAsync(c => c.ColorCode == colorCode);
        }

        public async Task<bool> IsColorExistsById(Guid colorId)
        {
            return await _context.ElectricVehicleColors
                .AnyAsync(c => c.Id == colorId);
        }

        public async Task<bool> IsColorExistsByName(string colorName)
        {
             return await _context.ElectricVehicleColors
                .AnyAsync(c => c.ColorName == colorName);
        }
    }
}
