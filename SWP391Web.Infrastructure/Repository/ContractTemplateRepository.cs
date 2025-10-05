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
    public class ContractTemplateRepository : Repository<EContractTemplate>, IContractTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        public ContractTemplateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EContractTemplate?> GetbyCodeAsync(string code, CancellationToken token)
            => await _context.ContractTemplates.Include("_versions").FirstOrDefaultAsync(v => v.Code == code, token);

        
    
    }
}
