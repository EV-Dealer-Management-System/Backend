using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.CustomerOrder;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;
using System.Security.Claims;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerOrderController : ControllerBase
    {
        public readonly ICustomerOrderService _customerOrderService;
        public CustomerOrderController(ICustomerOrderService customerOrderService)
        {
            _customerOrderService = customerOrderService ?? throw new ArgumentNullException(nameof(customerOrderService));
        }

        [HttpPost("create-customer-order")]
        public async Task<ActionResult<ResponseDTO>> CreateCustomerOrderAsync([FromBody] CreateCustomerOrderDTO createCustomerOrderDTO, CancellationToken ct)
        {
            var response = await _customerOrderService.CreateCustomerOrderAsync(User, createCustomerOrderDTO, ct);
            return StatusCode(response.StatusCode,response);
        }
    }
}
