using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Quote;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IQuoteService
    {
        Task<ResponseDTO> GetAllAsync(ClaimsPrincipal user);
        Task<ResponseDTO> GetQuoteByIdAsync(ClaimsPrincipal user , Guid id);
        Task<ResponseDTO> CreateQuoteAsync(ClaimsPrincipal user , CreateQuoteDTO createQuoteDTO);
        Task<ResponseDTO> UpdateQuoteStatusAsync(ClaimsPrincipal user , Guid id, QuoteStatus newStatus);

    }
}
