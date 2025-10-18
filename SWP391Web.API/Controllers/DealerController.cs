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
        public DealerController(IDealerService dealerService)
        {
            _dealerService = dealerService;
        }

        [HttpPost]
        [Route("create-dealer-staff")]
        [Authorize(Roles = StaticUserRole.DealerManager)]
        public async Task<IActionResult> CreateDealerStaff([FromBody] CreateDealerStaffDTO createDealerStaffDTO, CancellationToken ct)
        {
            var response = await _dealerService.CreateDealerStaffAsync(User, createDealerStaffDTO, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
