using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface ICustomerOrderRepository : IRepository<CustomerOrder>
    {
        Task<CustomerOrder?> GetByIdAsync(Guid customerOrderId);
        Task<bool>? IsExistByIdAsync(Guid id);
        Task<CustomerOrder?> GetByOrderNoAsync(int customerOrderNo);
        int GenerateOrderNumber();
        Task<CustomerOrder?> GetByEContractId(Guid eContractId, CancellationToken ct);
        Task<List<CustomerOrder>?> GetAllCustomerOrderDeposit(CancellationToken ct);
        Task<List<CustomerOrder>?> GetAllCustomerOrderPending(CancellationToken ct);
    }
}
