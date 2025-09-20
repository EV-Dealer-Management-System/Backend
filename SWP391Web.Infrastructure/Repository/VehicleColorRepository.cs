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
    public class VehicleColorRepository : Repository<VehicleColor>, IVehicleColorRepository
    {
        public readonly ApplicationDbContext _context;
        public VehicleColorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<VehicleColor?> GetByIdAsync(Guid id)
        {
            return await _context.VehicleColors
                .Include(c => c.ColorId)
                .FirstOrDefaultAsync(c => c.ColorId == id);
        }

        public async Task<VehicleColor?> GetByNameAsync(string colorName)
        {
            return await _context.VehicleColors
                .Include(c => c.ColorName)
                .FirstOrDefaultAsync(c => c.ColorName == colorName);
        }

        public async Task<bool> IsExistByColorName(string colorName)
        {
           return await _context.VehicleColors
                .AnyAsync(c => c.ColorName == colorName);
        }
    }
}
