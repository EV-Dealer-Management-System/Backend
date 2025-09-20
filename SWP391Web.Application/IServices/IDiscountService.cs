using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DisCount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IDiscountService
    {
        Task<ResponseDTO> GetAllDiscounts
            (
            string? filterOn,
            string? filterQuery,
            string? sortBy,
            bool? isAcsending
            );
        Task<ResponseDTO> GetDiscountByIdAsync(Guid id);
        Task<ResponseDTO> CreateDiscountAsync(CreateDiscountDTO createDiscountDTO);
        Task<ResponseDTO> UpdateDiscountAsync(Guid discountId, UpdateDiscountDTo updateDiscountDTO);
        Task<ResponseDTO> DeleteDiscountAsync(Guid discountId);
        Task<ResponseDTO> GetDiscountByCode(string code);
        
        

    }
}
