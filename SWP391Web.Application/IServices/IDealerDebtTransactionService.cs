using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerDebt;
using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IDealerDebtTransactionService
    {
        Task<ResponseDTO> CraeteDealerDebtTransaction(CreateDealerDebtTransactionDTO dealerDebtTransactionDTO, CancellationToken ct);
        Task<ResponseDTO<List<DealerDebtTransaction>>> GetAll(Guid dealerId, DateTime fromUtc, DateTime toUtc, int pageNumber, int pageSize, CancellationToken ct);
    }
}
