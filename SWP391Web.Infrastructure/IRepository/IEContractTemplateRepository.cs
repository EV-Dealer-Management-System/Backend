using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IEContractTemplateRepository: IRepository<EContractTemplate>
    {
        Task<EContractTemplate?> GetbyCodeAsync(string code, CancellationToken token);
        Task<EContractTemplate?> GetbyIdAsync(Guid id, CancellationToken token);
    }
}
