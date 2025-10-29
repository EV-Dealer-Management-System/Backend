using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IDealerFeedbackRepository : IRepository<DealerFeedback>
    {
        Task<DealerFeedback?> GetFeedbackByIdAsync(Guid id);
        Task<List<DealerFeedback>> GetFeedbacksByDealerIdAsync(Guid dealerId);
        Task<List<DealerFeedback>> GetAllDealerFeedbacksWithDetailAsync(CancellationToken ct);

    }
}
