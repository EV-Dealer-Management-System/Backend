using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Promotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IPromotionService
    {
        Task<ResponseDTO> GetPromotionByIdAsync(Guid promotionId);
        Task<ResponseDTO> GetAllAsync();
        Task<ResponseDTO> CreatePromotionAsync(CreatePromotionDTO createPromotionDTO);
        Task<ResponseDTO> UpdatePromotionAsync(Guid promotionId,UpdatePromotionDTO updatePromotionDTO);
        Task<ResponseDTO> DeletePromotionAsync(Guid promotionId);
        Task<ResponseDTO> GetPromotionByNameAsync(string name);
    }
}
