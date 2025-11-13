using Amazon.S3.Model.Internal.MarshallTransformations;
using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Dealer;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Application.Services
{
    public class DealerDailyInventoryService : IDealerDailyInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DealerDailyInventoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

        public async Task<ResponseDTO> GetDemandSeriesAsync(Guid dealerId, Guid evTemplateId, DateTime from, DateTime to, CancellationToken ct)
        {
            try
            {
                var fromDate = from.Date;
                var toDate = to.Date;

                var data = await _unitOfWork.DealerDailyInventoryRepository.GetRangeAsync(dealerId, evTemplateId, fromDate, toDate, ct);

                var getData = _mapper.Map<List<DemandSeriesPointDTO>>(data);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Demand series retrieved successfully.",
                    StatusCode = 200,
                    Result = getData
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error retrieving demand series: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpsertForecastBatchAsync(IEnumerable<UpsertDealerInventoryForecastDTO> forecasts, CancellationToken ct)
        {
            try
            {
                var now = DateTime.UtcNow;

                var rows = forecasts.Select(f => new DealerInventoryForecast
                {
                    Id = Guid.NewGuid(),
                    DealerId = f.DealerId,
                    EVTemplateId = f.EVTemplateId,
                    TargetDate = f.TargetDate.Date,
                    Forecast = f.Forecast,
                    ForecastLower = f.ForecastLower,
                    ForecastUpper = f.ForecastUpper,
                    CreatedAtUtc = now,
                    ModelVersion = f.ModelVersion
                }).ToList();

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _unitOfWork.DealerInventoryForecastRepository.UpsertRangeAsync(rows, ct);
                    await _unitOfWork.SaveAsync(ct);
                }, ct);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Forecast batch upserted successfully.",
                    StatusCode = 200,
                    Result = new
                    {
                        Count = rows.Count
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error upserting forecast batch: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
