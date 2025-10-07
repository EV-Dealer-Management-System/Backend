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
    public class EContractTemplateRepository : Repository<EContractTemplate>, IEContractTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        public EContractTemplateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<EContractTemplate?> GetByCodeAsync(string code, CancellationToken ct)
        {
            return await _context.EContractTemplates
                .Include(et => et.Versions)
                .SingleAsync(et => et.Code == code, ct);
        }
    }
}
