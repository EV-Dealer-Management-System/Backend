using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.CustomerOrder;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;
using SWP391Web.Domain.Enums;
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

        [HttpPost]
        [Route("create-customer-order")]
        public async Task<ActionResult<ResponseDTO>> CreateCustomerOrderAsync([FromBody] CreateCustomerOrderDTO createCustomerOrderDTO, CancellationToken ct)
        {
            var response = await _customerOrderService.CreateCustomerOrderAsync(User, createCustomerOrderDTO, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-all-customer-orders")]
        public async Task<ActionResult<ResponseDTO>> GetAllCustomerOrdersAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] OrderStatus? orderStatus = default, CancellationToken ct = default)
        {
            var response = await _customerOrderService.GetAllCustomerOrders(User, pageNumber, pageSize, orderStatus, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("cancel-customer-order/{customerOrderId}")]
        public async Task<ActionResult<ResponseDTO>> CancelCustomerOrderAsync([FromRoute] Guid customerOrderId, CancellationToken ct)
        {
            var response = await _customerOrderService.CancelCustomerOrderAsync(customerOrderId, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("pay-deposit-customer-order/{customerOrderId}")]
        public async Task<ActionResult<ResponseDTO>> PayDepositCustomerOrderAsync([FromRoute] Guid customerOrderId, [FromRoute] bool isCash, CancellationToken ct)
        {
            var response = await _customerOrderService.PayDeposit(customerOrderId, isCash, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("confirm-customer-order/{customerOrderId:guid}")]
        public async Task<ActionResult<ResponseDTO>> ConfirmCustomerOrder(Guid customerOrderId, [FromQuery] string email, [FromQuery] bool isAccept, CancellationToken ct)
        {
            var response = await _customerOrderService.CustomerConfirm(customerOrderId, email, isAccept, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("pay-customer-order")]
        public async Task<ActionResult<ResponseDTO>> PayCustomerOrder([FromBody] ConfirmCustomerOrderDTO confirmCustomerOrderDTO, CancellationToken ct)
        {
            var response = await _customerOrderService.PayCustomerOrder(User, confirmCustomerOrderDTO, ct);
            return StatusCode(response.StatusCode, response);
        }

    }
}
