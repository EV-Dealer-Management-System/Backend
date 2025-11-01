using Aspose.Words;
using Microsoft.AspNetCore.Http.HttpResults;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Dealer;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class DealerDebtService : IDealerDebtService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DealerDebtService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> AddPurchaseForDealerAsync(Guid dealerId, RecordDebtDTO debtDTO, CancellationToken ct)
        {
            try
            {
                var confirmDateUtc = debtDTO.ConfirmDateUtc;
                var year = debtDTO.ConfirmDateUtc.Year;
                var quarter = GetQuarter(confirmDateUtc);

                if (IsLastMonthOfQuarter(confirmDateUtc))
                {
                    (year, quarter) = MoveToNextQuarter(confirmDateUtc);
                }

                var (periodFrom, periodTo) = _unitOfWork.DealerDebtRepository.GetQuarterRangeUtc(confirmDateUtc);

                var debt = await _unitOfWork.DealerDebtRepository
                    .GetByDealerAndPeriodAsync(dealerId, periodFrom, periodTo, ct);

                if (debt == null)
                {
                    decimal openingBalance = 0m;

                    var lastDebt = await _unitOfWork.DealerDebtRepository
                        .GetLatestByDealerAsync(dealerId, ct);

                    if (lastDebt != null)
                    {
                        openingBalance = lastDebt.ClosingBalance;
                    }

                    debt = new DealerDebt
                    {
                        Id = Guid.NewGuid(),
                        DealerId = dealerId,
                        PeriodFrom = periodFrom,
                        PeriodTo = periodTo,
                        OpeningBalance = openingBalance,
                        PurchasesAmount = 0m,
                        PaymentsAmount = 0m,
                        CommissionsAmount = 0m,
                        PenaltiesAmount = 0m,
                        ClosingBalance = openingBalance,
                        ReferenceNo = debtDTO.ReferenceNo,
                        Note = $"Auto-create debt for Q{quarter}/{year}",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.DealerDebtRepository.AddAsync(debt, ct);
                }

                debt.PurchasesAmount += debtDTO.Amount;

                Recalc(debt);

                _unitOfWork.DealerDebtRepository.Update(debt);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Successfully added purchase for dealer debt."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"Failed to add purchase for dealer debt: {ex.Message}"
                };
            }
        }


        private int GetQuarter(DateTime date)
        {
            return ((date.Month - 1) / 3) + 1;
        }

        private static void Recalc(DealerDebt debt)
        {
            var raw = debt.ClosingBalance = debt.OpeningBalance + debt.PurchasesAmount - debt.PaymentsAmount - debt.CommissionsAmount + debt.PenaltiesAmount;
            //if (raw < 0)
            //{
            //    debt.ClosingBalance = 0;
            //    debt.OverpaidAmount = Math.Abs(raw);
            //}
            //else
            //{
            //    debt.ClosingBalance = raw;
            //    debt.OverpaidAmount = 0;
            //}
        }



        public static bool IsLastMonthOfQuarter(DateTime dt)
        {
            return dt.Month % 3 == 0;
        }

        public static (int year, int quarter) MoveToNextQuarter(DateTime asOfUtc)
        {
            var quarter = (asOfUtc.Month - 1) / 3 + 1;
            if (quarter == 4)
                return (asOfUtc.Year + 1, 1);

            return (asOfUtc.Year, quarter + 1);
        }
    }
}
