using Microsoft.EntityFrameworkCore;
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
    public class EVTemplateRepository : Repository<EVTemplate>, IEVTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        public EVTemplateRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<EVTemplate?> GetByIdAsync(Guid EVTemplateId)
        {
            return await _context.EVTemplates.FirstOrDefaultAsync(evt => evt.Id == EVTemplateId);
        }

        public async Task<bool>? IsEVTemplateExistsById(Guid EVTemplateId)
        {
            return await _context.EVTemplates.AnyAsync(evt => evt.Id == EVTemplateId);
        }
    }
}
