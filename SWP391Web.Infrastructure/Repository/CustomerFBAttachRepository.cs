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
    internal class CustomerFBAttachRepository : Repository<CustomerFBAttachment>, ICustomerFBAttachRepository
    {
        private readonly ApplicationDbContext _context;
        public CustomerFBAttachRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<CustomerFBAttachment>? GetAttachmentsByCustomerFbId(Guid customerFbId)
        {
            return _context.CustomerFBAttachments
                .Where(attach => attach.CustomerFeedBackId == customerFbId)
                .ToList();
        }
    }
}
