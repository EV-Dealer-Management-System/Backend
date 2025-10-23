using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Dealer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IDealerService
    {
        Task<ResponseDTO> CreateDealerStaffAsync(ClaimsPrincipal user, CreateDealerStaffDTO createDealerStaffDTO, CancellationToken ct);
        Task<ResponseDTO> GetAllDealerStaffAsync(ClaimsPrincipal claimUser, string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending, int pageNumber, int PageSize, CancellationToken ct);
        Task<ResponseDTO> GetAllDealerAsync(string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending, int pageNumber, int PageSize, CancellationToken ct);
    }

}
