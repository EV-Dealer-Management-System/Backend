using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Infrastructure.Repository
{
    public class EmailTemplateRepository : Repository<EmailTemplate>, IEmailTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        public EmailTemplateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EmailTemplate?> GetByNameAsync(string name)
        {
            return await _context.EmailTemplates.FirstOrDefaultAsync(e => e.TemplateName == name);
        }
    }
}
