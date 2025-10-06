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
        public IContractTemplateRepository ContractTemplateRepository { get; }
        public IDealerRepository DealerRepository { get; }
        public IElectricVehicleColorRepository ElectricVehicleColorRepository { get; }
        public IElectricVehicleModelRepository ElectricVehicleModelRepository { get; }
        public IElectricVehicleVersionRepository ElectricVehicleVersionRepository { get; }
        public IElectricVehicleRepository ElectricVehicleRepository { get; }
        public IEContractTemplateRepository EContractTemplateRepository { get; }
        public IEContractTermRepository EContractTermRepository { get; }
        Task<int> SaveAsync();
    }
}
