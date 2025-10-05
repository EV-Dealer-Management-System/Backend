using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IContractTemplateRepository: IRepository<EContractTemplate>
    {
        Task<EContractTemplate?> GetbyCodeAsync(string code, CancellationToken token);
    }
}
