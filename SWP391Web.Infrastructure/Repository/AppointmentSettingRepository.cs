using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Repository
{
    public class AppointmentSettingRepository : Repository<AppointmentSetting>, IAppointmentSettingRepository
    {
        public readonly ApplicationDbContext _context;
        public AppointmentSettingRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<AppointmentSetting?> GetByDealerIdAsync(Guid dealerId)
        {
            return await _context.AppointmentSettings
                .FirstOrDefaultAsync(a => a.DealerId == dealerId);
        }

        public async Task<AppointmentSetting?> GetById(Guid id)
        {
            return await _context.AppointmentSettings
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public Task<AppointmentSetting?> GetDefaultAsync()
        {
            return _context.AppointmentSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.DealerId == null && a.ManagerId == null);
        }
    }
}
