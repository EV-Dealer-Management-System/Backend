using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerDebt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IDealerDebtService
    {
        Task<ResponseDTO> AddPurchaseForDealerAsync(Guid dealerId, RecordDebtDTO debtDTO, CancellationToken ct);
        Task<ResponseDTO> AddPaymentForDealerAsync(Guid dealerId, RecordPaymentDTO paymentDTO, CancellationToken ct);
        Task<ResponseDTO> AddCommissionForDealerAsync(Guid dealerId, RecordCommissionDTO dto, CancellationToken ct);
    }
}
