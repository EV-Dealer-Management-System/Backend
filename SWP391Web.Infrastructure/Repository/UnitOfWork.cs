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
        public IDealerRepository DealerRepository { get; private set; }
        public IElectricVehicleColorRepository ElectricVehicleColorRepository { get; private set; }
        public IElectricVehicleModelRepository ElectricVehicleModelRepository { get; private set; }
        public IElectricVehicleVersionRepository ElectricVehicleVersionRepository { get; private set; }
        public IElectricVehicleRepository ElectricVehicleRepository { get; private set; }
        public IEContractTemplateRepository EContractTemplateRepository { get; private set; }
        public IEContractTermRepository EContractTermRepository { get; private set; }
        public IEContractRepository EContractRepository { get; private set; }
        public IBookingEVRepository BookingEVRepository { get; private set; }
        public IEVCInventoryRepository EVCInventoryRepository { get; private set; }
        public IWarehouseRepository WarehouseRepository { get; private set; }
        public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManagerRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            UserManagerRepository = new UserManagerRepository(userManagerRepository, _context);
            CustomerRepository = new CustomerRepository(_context);
            EmailTemplateRepository = new EmailTemplateRepository(_context);
            CustomerOrderRepository = new CustomerOrderRepository(_context);
            DealerRepository = new DealerRepository(_context);
            ElectricVehicleColorRepository = new ElectricVehicleColorRepository(_context);
            ElectricVehicleModelRepository = new ElectricVehicleModelRepository(_context);
            ElectricVehicleVersionRepository = new ElectricVehicleVersionRepository(_context);
            ElectricVehicleRepository = new ElectricVehicleRepository(_context);
            EContractTemplateRepository = new EContractTemplateRepository(_context);
            EContractTermRepository = new EContractTermRepository(_context);
            EContractRepository = new EContractRepository(_context);
            BookingEVRepository = new BookingEVRepository(_context);
            EVCInventoryRepository = new EVCInventoryRepository(_context);
            WarehouseRepository = new WarehouseRepository(_context);
        }
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
