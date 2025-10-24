using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Customer;
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
        [HttpPost("create-customer")]
        public async Task<ActionResult<ResponseDTO>> CreateCustomer([FromBody] CreateCustomerDTO createCustomerDTO)
        {
            var response = await _customerService.CreateCustomerAsync(User, createCustomerDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-customers-by-id/{customerId}")]
        public async Task<ActionResult<ResponseDTO>> GetCustomersById(Guid customerId)
        {
            var response = await _customerService.GetCustomerByIdAsync(User, customerId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-customers")]
        public async Task<ActionResult<ResponseDTO>> GetAllCustomers()
        {
            var response = await _customerService.GetAllCustomerAsync(User);
            return StatusCode(response.StatusCode, response);
        }
    }
}
