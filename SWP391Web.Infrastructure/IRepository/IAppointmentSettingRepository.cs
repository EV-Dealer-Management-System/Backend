using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IAppointmentSettingRepository :  IRepository<AppointmentSetting>
    {
        Task<AppointmentSetting> GetByDealerIdAsync(Guid dealerId);
        Task<AppointmentSetting> GetDefaultAsync(Guid id);
    }
}
