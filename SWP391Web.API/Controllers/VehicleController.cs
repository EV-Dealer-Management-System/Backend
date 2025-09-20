using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Vehicle;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }
        [HttpPost]
        [Route("create-vehicle")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> CreateVehicleAsync([FromBody] CreateVehicleDTO vehicleCreateDTO)
        {
            var response = await _vehicleService.CreateVehicleAsync(vehicleCreateDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-vehicle/{vehicleid}")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> UpdateVehicleAsync(Guid vehicleid, [FromBody] UpdateVehicleDTO updateVehicleDTO)
        {
            var response = await _vehicleService.UpdateVehicleAsync(vehicleid, updateVehicleDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-vehicles/{promotionId}")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> GetVehiclesByIdAsync([FromRoute] Guid vehicleId)
        {
            var response = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-all-vehicles")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> GetAllVehicles(
            [FromQuery] string? filterOn,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] bool? isAscending
            )
        {
            var response = await _vehicleService.GetAllVehicles(filterOn, filterQuery, sortBy, isAscending);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-vehicle/{vehicleid}")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> DeleteVehicleAsync(Guid vehicleid)
        {
            var response = await _vehicleService.DeleteVehicleAsync(vehicleid);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-vehicle-by-name/{name}")]
        [Authorize(Roles = StaticUserRole.Manager)]
        public async Task<ActionResult<ResponseDTO>> GetVehicleByName([FromRoute] string name)
        {
            var response = await _vehicleService.GetVehicleByName(name);
            return StatusCode(response.StatusCode, response);
        }
    }
}
