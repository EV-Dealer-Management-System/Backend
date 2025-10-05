using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricVehicleController : ControllerBase
    {
        public readonly IElectricVehicleService _electricVehicleService;
        public ElectricVehicleController(IElectricVehicleService electricVehicleService)
        {
            _electricVehicleService = electricVehicleService ?? throw new ArgumentNullException(nameof(electricVehicleService));
        }
        [HttpPost("create-vehicle")]
        public async Task<ActionResult<ResponseDTO>> CreateElectricVehicleAsync([FromBody] CreateElecticVehicleDTO createElecticVehicleDTO)
        {
            var response = await _electricVehicleService.CreateVehicleAsync(createElecticVehicleDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-vehicles")]
        public async Task<ActionResult> GetAllVehiclesAsync()
        {
            var response = await _electricVehicleService.GetAllVehiclesAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-vehicle-by-id/{vehicleId}")]
        public async Task<ActionResult> GetVehicleByIdAsync([FromRoute] Guid vehicleId)
        {
            var response = await _electricVehicleService.GetVehicleByIdAsync(vehicleId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-vehicle-by-vin/{vehicleVIN}")]
        public async Task<ActionResult> GetVehicleByVINAsync([FromRoute] string vehicleName)
        {
            var response = await _electricVehicleService.GetVehicleByVinAsync(vehicleName);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-vehicle")]
        public async Task<ActionResult> UpdateVehicleAsync(Guid vehicleId, [FromBody] UpdateElectricVehicleDTO updateElectricVehicleDTO)
        {
            var response = await _electricVehicleService.UpdateVehicleAsync(vehicleId, updateElectricVehicleDTO);
            return StatusCode(response.StatusCode, response);
        }

    }
}
