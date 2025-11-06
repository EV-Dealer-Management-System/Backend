using SWP391Web.Domain.Entities;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface ICustomerOrderRepository : IRepository<CustomerOrder>
    {
        Task<CustomerOrder?> GetByIdAsync(Guid customerOrderId);
        Task<bool>? IsExistByIdAsync(Guid id);
        Task<CustomerOrder?> GetByOrderNoAsync(int customerOrderNo);
        int GenerateOrderNumber();
    }
}
