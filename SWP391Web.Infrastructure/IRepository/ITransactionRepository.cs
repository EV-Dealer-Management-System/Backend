using SWP391Web.Domain.Entities;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<bool> IsExistTransactionAsync(string method, string orderRef, CancellationToken ct);
    }
}
