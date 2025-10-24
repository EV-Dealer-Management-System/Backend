using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.CustomerOrder;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerOrderController : ControllerBase
    {
        private readonly ICustomerOrderService _customerOrderService;
        public CustomerOrderController(ICustomerOrderService customerOrderService)
        {
            _customerOrderService = customerOrderService;
        }
        [HttpPost]
        [Route("create-customer-order")]
        public async Task<IActionResult> CreateCustomerOrder([FromBody] CreateOrderDTO createOrderDTO, CancellationToken ct)
        {
            var response = await _customerOrderService.CreateCustomerOrderAsync(User, createOrderDTO, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
