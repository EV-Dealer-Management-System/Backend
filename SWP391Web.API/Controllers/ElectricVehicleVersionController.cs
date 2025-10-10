using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicleVersion;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricVehicleVersionController : ControllerBase
    {
        public readonly IElectricVehicleVersionService _electricVehicleVersionService;
        public ElectricVehicleVersionController(IElectricVehicleVersionService electricVehicleVersionService)
        {
            _electricVehicleVersionService = electricVehicleVersionService ?? throw new ArgumentNullException(nameof(electricVehicleVersionService));
        }
        [HttpPost("create-version")]
        public async Task<ActionResult<ResponseDTO>> CreateVersionAsync([FromBody] CreateElectricVehicleVersionDTO createElectricVehicleVersionDTO)
        {
            var response = await _electricVehicleVersionService.CreateVersionAsync(createElectricVehicleVersionDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-versions-by-modelid/{modelId}")]
        public async Task<ActionResult<ResponseDTO>> GetAllVersionsByModelIdAsync(Guid modelId)
        {
            var response = await _electricVehicleVersionService.GetAllVersionsByModelIdAsync(modelId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-version-by-id/{versionId}")]
        public async Task<ActionResult<ResponseDTO>> GetVersionByIdAsync([FromRoute] Guid versionId)
        {
            var response = await _electricVehicleVersionService.GetVersionByIdAsync(versionId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-version-by-name/{versionName}")]
        public async Task<ActionResult<ResponseDTO>> GetVersionByNameAsync([FromRoute] string versionName)
        {
            var response = await _electricVehicleVersionService.GetVersionByNameAsync(versionName);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-version/{versionId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateVersionAsync(Guid versionId, [FromBody] UpdateElectricVehicleVersionDTO updateElectricVehicleVersionDTO)
        {
            var response = await _electricVehicleVersionService.UpdateVersionAsync(versionId, updateElectricVehicleVersionDTO);
            return StatusCode(response.StatusCode, response);
        }

    }
}
