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
    public class EVAttachmentRepository : Repository<EVAttachment>, IEVAttachmentRepository
    {
        private readonly ApplicationDbContext _context;
        public EVAttachmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<EVAttachment>? GetAttachmentsByElectricVehicleId(Guid electricVehicleId)
        {
            return  _context.EVAttachments
                .Where(att => att.ElectricVehicleId == electricVehicleId).ToList();
        }
    }
}
