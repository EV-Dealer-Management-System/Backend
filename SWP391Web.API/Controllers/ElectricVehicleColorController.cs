using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicleColor;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricVehicleColorController : ControllerBase
    {
        public readonly IElectricVehicleColorService _electricVehicleColorService;
        public ElectricVehicleColorController(IElectricVehicleColorService electricVehicleColorService)
        {
            _electricVehicleColorService = electricVehicleColorService ?? throw new ArgumentNullException(nameof(electricVehicleColorService));
        }
        [HttpPost("create-color")]
        public async Task<ActionResult<ResponseDTO>> CreateColor([FromBody] CreateElectricVehicleColorDTO createElectricVehicleColorDTO)
        {
            var response = await _electricVehicleColorService.CreateColorAsync(createElectricVehicleColorDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-all-colors")]
        public async Task<ActionResult<ResponseDTO>> GetAllColors()
        {
            var response = await _electricVehicleColorService.GetAllColorsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-available-colors-by-modelid-and-versionid/{modelId}/{versionId}")]
        public async Task<ActionResult<ResponseDTO>> GetAllColorsByModelIdAndVersionId([FromRoute] Guid modelId, [FromRoute] Guid versionId)
        {
            var response = await _electricVehicleColorService.GetAvailableColorsByModelIdAndVersionIdAsync(modelId, versionId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-color-by-id/{colorId}")]
        public async Task<ActionResult<ResponseDTO>> GetColorById([FromRoute] Guid colorId)
        {
            var response = await _electricVehicleColorService.GetColorByIdAsync(colorId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-color-by-name/{colorName}")]
        public async Task<ActionResult<ResponseDTO>> GetColorByName([FromRoute] string colorName)
        {
            var response = await _electricVehicleColorService.GetColorByNameAsync(colorName);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-color-by-code/{colorCode}")]
        public async Task<ActionResult<ResponseDTO>> GetColorByCode([FromRoute] string colorCode)
        {
            var response = await _electricVehicleColorService.GetColorByCodeAsync(colorCode);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-color/{colorId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateColor([FromRoute] Guid colorId, [FromBody] UpdateElectricVehicleColor updateElectricVehicleColor)
        {
            var response = await _electricVehicleColorService.UpdateColorAsync(colorId, updateElectricVehicleColor);
            return StatusCode(response.StatusCode, response);
        }
    }
}
