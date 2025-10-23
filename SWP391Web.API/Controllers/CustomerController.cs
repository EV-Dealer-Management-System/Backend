using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        [Authorize]
        [HttpGet]
        [Route("get-user-profile")]
        public async Task<ActionResult<ResponseDTO>> GetCustomersProfile()
        {
            var response = await _customerService.GetUserProfile(User);
            return StatusCode(response.StatusCode, response);
        }
    }
}
