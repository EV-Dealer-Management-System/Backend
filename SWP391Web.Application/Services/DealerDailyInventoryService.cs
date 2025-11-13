using Amazon.S3.Model.Internal.MarshallTransformations;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Application.Services
{
    public class DealerDailyInventoryService : IDealerDailyInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DealerDailyInventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> BuildDailySnapshotAsync(DateTime utcDate, CancellationToken ct)
        {
            try
            {
                var dateOnly = utcDate.Date;
                var dateOnlyPrev = dateOnly.AddDays(-1);

                var pairs = await _unitOfWork.EVTemplateRepository.GetActiveDealerTemplatePairsAsync(ct);
                var openingMap = await _unitOfWork.DealerDailyInventoryRepository.GetClosingStockMapAsync(dateOnlyPrev, ct);
                var inflowMap = await _unitOfWork.DealerDailyInventoryRepository.GetInflowAsync(dateOnly, ct);
                var outflowMap = await _unitOfWork.DealerDailyInventoryRepository.GetOutflowAsync(dateOnly, ct);

                var rows = new List<DealerDailyInventory>();
                foreach (var (dealerId, templateId) in pairs)
                {
                    var opening = openingMap.TryGetValue((dealerId, templateId), out var open) ? open : 0;
                    var inflow = inflowMap.TryGetValue((dealerId, templateId), out var inf) ? inf : 0;
                    var outflow = outflowMap.TryGetValue((dealerId, templateId), out var outf) ? outf : 0;
                    var closing = opening + inflow - outflow;

                    rows.Add(new DealerDailyInventory
                    {
                        DealerId = dealerId,
                        EVTemplateId = templateId,
                        SnapshotDate = dateOnly,
                        OpeningStock = opening,
                        Inflow = inflow,
                        Outflow = outflow,
                        ClosingStock = closing
                    });
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _unitOfWork.DealerDailyInventoryRepository.UpsertRangeAsync(rows, ct);
                    await _unitOfWork.SaveAsync(ct);
                }, ct);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Daily snapshot built successfully.",
                    StatusCode = 200,
                    Result = new
                    {
                        Rows = rows,
                        Count = rows.Count
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error building daily snapshot: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
