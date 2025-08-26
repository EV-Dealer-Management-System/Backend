using SWP391Web.Domain.Entities;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IEmailTemplateRepository : IRepository<EmailTemplate>
    {
        Task<EmailTemplate?> GetByNameAsync(string name);
    }
}
