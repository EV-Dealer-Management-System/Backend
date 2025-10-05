using Microsoft.AspNetCore.Identity;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IUserManagerRepository UserManagerRepository { get; private set; }
        public ICustomerRepository CustomerRepository { get; private set; }
        public IEmailTemplateRepository EmailTemplateRepository { get; private set; }
        public ICustomerOrderRepository CustomerOrderRepository { get; private set; }
        public IContractTemplateRepository ContractTemplateRepository { get; private set; }
        public IDealerRepository DealerRepository { get; private set; }
        public IElectricVehicleColorRepository ElectricVehicleColorRepository { get; private set; }
        public IElectricVehicleModelRepository ElectricVehicleModelRepository { get; private set; }
        public IElectricVehicleVersionRepository ElectricVehicleVersionRepository { get; private set; }
        public IElectricVehicleRepository ElectricVehicleRepository { get; private set; }

        public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManagerRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            UserManagerRepository = new UserManagerRepository(userManagerRepository);
            CustomerRepository = new CustomerRepository(_context);
            EmailTemplateRepository = new EmailTemplateRepository(_context);
            CustomerOrderRepository = new CustomerOrderRepository(_context);
            ContractTemplateRepository = new ContractTemplateRepository(_context);
            DealerRepository = new DealerRepository(_context);
            ElectricVehicleColorRepository = new ElectricVehicleColorRepository(_context);
            ElectricVehicleModelRepository = new ElectricVehicleModelRepository(_context);
            ElectricVehicleVersionRepository = new ElectricVehicleVersionRepository(_context);
            ElectricVehicleRepository = new ElectricVehicleRepository(_context);
        }
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
