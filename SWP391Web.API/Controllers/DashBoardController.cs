using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly IDashBoardService _dashBoardService;
        public DashBoardController(IDashBoardService dashBoardService)
        {
            _dashBoardService = dashBoardService;
        }
        [HttpGet("total-customer")]
        public async Task<IActionResult> GetTotalCustomerAsync()
        {
            var response = await _dashBoardService.GetTotalCustomerAsync();
            return StatusCode(response.StatusCode, response);
        }
    }
}
