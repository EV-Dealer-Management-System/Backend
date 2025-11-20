using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface ILogRepository : IRepository<Log>
    {
        Task<List<Log>> GetLogsByCreateAtRange(DateTime startDate , DateTime endDate);
        Task<List<Log>> GetLogsByDealerId(Guid dealerId);
        Task<List<Log>> GetLogsByType(LogType logType);
    }
}
