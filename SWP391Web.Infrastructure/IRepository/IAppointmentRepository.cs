using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<Appointment?> GetByIdAsync(Guid appointmentId);
        Task<List<Appointment>> GetByDealerIdAsync(Guid dealerId);
        Task<List<Appointment>> GetByCustomerIdAsync(Guid customerId);
    }
}
