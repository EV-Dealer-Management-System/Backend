using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Repository
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public NotificationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
