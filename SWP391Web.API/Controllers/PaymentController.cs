using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Payment;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [Route("create-vnpay/{customerOrderId:guid}")]
        public async Task<ActionResult<ResponseDTO>> CreateVNPayLink(Guid customerOrderId, CancellationToken ct)
        {
            var response = await _paymentService.CreateVNPayLink(customerOrderId, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("IPN")]
        public async Task<IActionResult> VNPayIPN([FromQuery] VNPayIPNDTO ipnDTO, CancellationToken ct)
        {
            var response = await _paymentService.HandleVNPayIpn(ipnDTO, ct);

            if (response.Result is VNPayIpnResponse ok)
            {
                return new JsonResult(ok);
            }

            return new JsonResult(new VNPayIpnResponse("99", "Unknown error"));
        }
    }
}
