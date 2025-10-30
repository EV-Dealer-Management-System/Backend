using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface ICustomerFeedbackRepository : IRepository<CustomerFeedback>
    {
        Task<CustomerFeedback?> GetFeedbackByIdAsync(Guid id);
        Task<List<CustomerFeedback>> GetFeedbacksByDealerIdAsync(Guid dealerId);
        Task<List<CustomerFeedback>> GetFeedbacksByCustomerIdAsync(Guid customerId);
        Task<List<CustomerFeedback>> GetAllCustomerFeedbacksWithDetailAsync(CancellationToken ct);

    }
}
