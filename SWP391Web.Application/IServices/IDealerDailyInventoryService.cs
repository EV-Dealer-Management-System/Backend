using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Dealer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IDealerDailyInventoryService
    {
        Task<ResponseDTO> BuildDailySnapshotAsync(DateTime utcDate, CancellationToken ct);
        Task<ResponseDTO> GetDemandSeriesAsync(Guid dealerId, Guid evTemplateId, DateTime from, DateTime to, CancellationToken ct);
        Task<ResponseDTO> UpsertForecastBatchAsync(IEnumerable<UpsertDealerInventoryForecastDTO> forecasts, CancellationToken ct);
    }
}
