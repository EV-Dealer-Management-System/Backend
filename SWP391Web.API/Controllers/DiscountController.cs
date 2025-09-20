using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DisCount;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;
        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }
        [HttpPost]
        [Route("create-discount")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> CreateDiscountAsync([FromBody] CreateDiscountDTO createDiscountDTO)
        {
            var response = await _discountService.CreateDiscountAsync(createDiscountDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-discount/{discountid}")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> GetDiscountByIdAsync([FromRoute] Guid discountid)
        {
            var response = await _discountService.GetDiscountByIdAsync(discountid);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-all-discounts")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> GetAllDiscounts(
            [FromQuery] string? filterOn,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] bool? isAcsending
            )
        {
            var response = await _discountService.GetAllDiscounts(filterOn, filterQuery, sortBy, isAcsending);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-discount/{discountid}")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> UpdateDiscountAsync(Guid discountid, [FromBody] UpdateDiscountDTo updateDiscountDTO)
        {
            var response = await _discountService.UpdateDiscountAsync(discountid, updateDiscountDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-discount/{discountid}")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> DeleteDiscountAsync(Guid discountid)
        {
            var response = await _discountService.DeleteDiscountAsync(discountid);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-discount-by-code/{code}")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> GetDiscountByCode([FromRoute] string code)
        {
            var response = await _discountService.GetDiscountByCode(code);
            return StatusCode(response.StatusCode, response);
        }
    }
}
