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
    public class DealerFBAttachmentRepository : Repository<DealerFBAttachment>, IDealerFBAttachmentRepository
    {
        public readonly ApplicationDbContext _context;
        public DealerFBAttachmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<DealerFBAttachment>? GetAttachmentsByDealerFbId(Guid dealerFbId)
        {
            return _context.DealerFBAttachments.Where(a => a.DealerFeedBackId == dealerFbId).ToList();
        }
    }
}
