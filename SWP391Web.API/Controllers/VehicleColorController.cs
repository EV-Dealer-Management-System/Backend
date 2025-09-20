using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.VehicleColorDTO;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleColorController : ControllerBase
    {
        private readonly IVehicleColorService _vehicleColorService;
        public VehicleColorController(IVehicleColorService vehicleColorService)
        {
            _vehicleColorService = vehicleColorService;
        }
        [HttpPost]
        [Route("create-vehicle-color")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<ResponseDTO>> CreateVehicleColorAsync([FromBody] CreateVehicleColorDTO vehicleColorCreateDTO)
        {
            var response = await _vehicleColorService.CreateVehicleColorAsync(vehicleColorCreateDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-vehiclecolor/{vehicleid}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<ResponseDTO>> GetVehicleColorByIdAsync([FromRoute] Guid vehicleid)
        {
            var response = await _vehicleColorService.GetVehicleColorByIdAsync(vehicleid);
            return StatusCode(response.StatusCode, response);
        }
    }
}
