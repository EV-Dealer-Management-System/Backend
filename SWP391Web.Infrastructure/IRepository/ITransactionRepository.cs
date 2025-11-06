using SWP391Web.Domain.Entities;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<bool> IsExistTransactionAsync(string method, string orderRef, CancellationToken ct);
        Task<decimal> GetDealerRevenueAsync(Guid dealerId, CancellationToken ct);
        Task<List<Transaction>> GetDealerTransactionsByYearAsync(Guid dealerId, int year, CancellationToken ct);

    }
}
