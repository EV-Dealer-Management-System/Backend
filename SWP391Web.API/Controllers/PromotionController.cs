﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Promotion;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;
using SWP391Web.Domain.Constants;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        public readonly IPromotionService _promotionService;
        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService ?? throw new ArgumentNullException(nameof(promotionService));
        }
        [HttpPost("create-promotion")]
        public async Task<ActionResult<ResponseDTO>> CreatePromotion([FromBody] CreatePromotionDTO createPromotionDTO)
        {
            var response = await _promotionService.CreatePromotionAsync(createPromotionDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("update-promotion/{promotionId}")]
        public async Task<ActionResult<ResponseDTO>> UpdatePromotion(Guid promotionId, [FromBody] UpdatePromotionDTO updatePromotionDTO)
        {
            var response = await _promotionService.UpdatePromotionAsync(promotionId, updatePromotionDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-promotion/{promotionId}")]
        public async Task<ActionResult<ResponseDTO>> GetPromotionById([FromRoute] Guid promotionId)
        {
            var response = await _promotionService.GetPromotionByIdAsync(promotionId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("delete-promotion/{promotionId}")]

        public async Task<ActionResult<ResponseDTO>> DeletePromotion([FromRoute] Guid promotionId)
        {
            var response = await _promotionService.DeletePromotionAsync(promotionId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-all-promotion")]
        public async Task<ActionResult<ResponseDTO>> GetAllPromotion()
        {
            var response = await _promotionService.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }
        

        
    }
}
