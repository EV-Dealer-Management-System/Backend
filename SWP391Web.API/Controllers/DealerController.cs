using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Dealer;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerController : ControllerBase
    {
        private readonly IDealerService _dealerService;
        private readonly IDealerTierService _dealerTierService;
        public DealerController(IDealerService dealerService, IDealerTierService dealerTierService)
        {
            _dealerService = dealerService;
            _dealerTierService = dealerTierService;
        }

        [HttpPost]
        [Route("create-dealer-staff")]
        [Authorize(Roles = StaticUserRole.DealerManager)]
        public async Task<IActionResult> CreateDealerStaff([FromBody] CreateDealerStaffDTO createDealerStaffDTO, CancellationToken ct)
        {
            var response = await _dealerService.CreateDealerStaffAsync(User, createDealerStaffDTO, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-all-dealer-staff")]
        [Authorize(Roles = StaticUserRole.DealerManager)]
        public async Task<IActionResult> GetAllDealerStaff([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy, [FromQuery] bool? isAcsending, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var response = await _dealerService.GetAllDealerStaffAsync(User, filterOn, filterQuery, sortBy, isAcsending, pageNumber, pageSize, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-all-dealers")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> GetAllDealers([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy, [FromQuery] bool? isAcsending, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var response = await _dealerService.GetAllDealerAsync(filterOn, filterQuery, sortBy, isAcsending, pageNumber, pageSize, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("update-dealer-tier/{dealerTierId}")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> UpdateDealerTier([FromRoute] Guid dealerTierId, [FromBody] UpdateDealerTierDTO updateDealer, CancellationToken ct)
        {
            var response = await _dealerTierService.UpdateDealerTier(dealerTierId, updateDealer, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-all-dealer-tiers")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> GetAllDealerTiers(CancellationToken ct)
        {
            var response = await _dealerTierService.GetAllDealerTiers(ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("create-dealer-policy-override/{dealerId}")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> CreateDealerPolicyOverride([FromRoute] Guid dealerId, [FromBody] CreateDealerPolicyOverrideDTO createDealerPolicy, CancellationToken ct)
        {
            var response = await _dealerTierService.CreateDealerPolicyOverrideAsync(dealerId, createDealerPolicy, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-effective-policy")]
        //[Authorize(Roles = StaticUserRole.Admin)]
        public async Task<IActionResult> GetEffectivePolicy([FromQuery] Guid dealerId, CancellationToken ct)
        {
            var response = await _dealerTierService.GetEffectivePolicyAsync(dealerId, ct);
            return StatusCode(200, response);
        }
    }
}
