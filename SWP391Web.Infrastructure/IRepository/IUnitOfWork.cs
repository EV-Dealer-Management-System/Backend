using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IUnitOfWork
    {
        public IUserManagerRepository UserManagerRepository { get; }
        public ICustomerRepository CustomerRepository { get; }
        public IEmailTemplateRepository EmailTemplateRepository { get; }
        public ICustomerOrderRepository CustomerOrderRepository { get; }
        public IDealerRepository DealerRepository { get; }
        public IElectricVehicleColorRepository ElectricVehicleColorRepository { get; }
        public IElectricVehicleModelRepository ElectricVehicleModelRepository { get; }
        public IElectricVehicleVersionRepository ElectricVehicleVersionRepository { get; }
        public IElectricVehicleRepository ElectricVehicleRepository { get; }
        public IEContractTemplateRepository EContractTemplateRepository { get; }
        public IEContractTermRepository EContractTermRepository { get; }
        public IEContractRepository EContractRepository { get; }
        public IBookingEVRepository BookingEVRepository { get; }
        public IEVCInventoryRepository EVCInventoryRepository { get; }
        public IWarehouseRepository WarehouseRepository { get; }
        public IQuoteRepository QuoteRepository { get; }
        public IPromotionRepository PromotionRepository { get; }
        public IEVAttachmentRepository EVAttachmentRepository { get; }
        public IDealerMemberRepository DealerMemberRepository { get; }
        Task<int> SaveAsync();
    }
}
