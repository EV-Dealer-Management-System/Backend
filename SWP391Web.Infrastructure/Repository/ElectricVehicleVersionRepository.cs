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
    public class ElectricVehicleVersionRepository : Repository<ElectricVehicleVersion>, IElectricVehicleVersionRepository
    {
        public readonly ApplicationDbContext _context;
        public ElectricVehicleVersionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public async Task<ElectricVehicleVersion?> GetByIdsAsync(Guid versionId)
        {
            return await _context.ElectricVehicleVersions
                .FirstOrDefaultAsync(v => v.Id == versionId);
        }

        public async Task<ElectricVehicleVersion?> GetByNameAsync(string versionName)
        {
            return await _context.ElectricVehicleVersions
                .FirstOrDefaultAsync(v => v.VersionName == versionName);
        }

        public async Task<bool> IsVersionExistsById(Guid versionId)
        {
            return await _context.ElectricVehicleVersions
                .AnyAsync(v => v.Id == versionId);
        }

        public async Task<bool> IsVersionExistsByName(string versionName)
        {
            return await _context.ElectricVehicleVersions
                .AnyAsync(v => v.VersionName == versionName);
        }
    }
}
