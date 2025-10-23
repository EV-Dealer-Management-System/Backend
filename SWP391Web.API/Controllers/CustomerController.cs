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
        [HttpGet]
        [Route("get-customers-by-id/{customerId}")]
        public async Task<ActionResult<ResponseDTO>> GetCustomersById(Guid customerId)
        {
            var response = await _customerService.GetCustomerById(User,customerId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
