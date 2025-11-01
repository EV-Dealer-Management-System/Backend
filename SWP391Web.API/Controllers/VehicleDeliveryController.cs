using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;
using SWP391Web.Domain.Enums;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleDeliveryController : ControllerBase
    {
        public readonly IVehicleDeliveryService _vehicleDeliveryService;
        public VehicleDeliveryController(IVehicleDeliveryService vehicleDeliveryService)
        {
            _vehicleDeliveryService = vehicleDeliveryService ?? throw new ArgumentNullException(nameof(vehicleDeliveryService));
        }
        [HttpGet("Get-all-deliveries/")]
        public async Task<ActionResult<ResponseDTO>> getAllVehicleDeliveries([FromQuery] DeliveryStatus? status)
        {
            var response = await _vehicleDeliveryService.GetAllVehicleDelivery(status);
            return StatusCode(response.StatusCode, response);
        }
    }
}
