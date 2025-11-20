using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IEContractRepository : IRepository<EContract>
    {
        Task<EContract?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<List<EContract>?> GetEContractDealerByDealerIdAsync(string managerId, CancellationToken ct);
        Task<EContract?> GetByBookingId(Guid bookingId, CancellationToken ct);

    }
}
