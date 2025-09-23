using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Application.IServices
{
    public interface ISmartCAService
    {
        Task<SmartCATransactionCreated> CreateTransactionAsync(SmartCACreateTxnInput input, CancellationToken token = default);
        Task<SmartCASignResult> GetTransactionResultAsync(string transactionId, CancellationToken token = default);
        Task<string> GetAccessTokenAsync(CancellationToken token = default);
    }
}
