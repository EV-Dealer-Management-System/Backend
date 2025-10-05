using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicleModel;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricVehicleModelController : ControllerBase
    {
        public readonly IElectricVehicleModelService _electricVehicleModelService;
        public ElectricVehicleModelController(IElectricVehicleModelService electricVehicleModelService)
        {
            _electricVehicleModelService = electricVehicleModelService ?? throw new ArgumentNullException(nameof(electricVehicleModelService));
        }
        [HttpPost("create-model")]
        public async Task<ActionResult<ResponseDTO>> CreateModel([FromBody] CreateElectricVehicleModelDTO createElectricVehicleModelDTO)
        {
            var response = await _electricVehicleModelService.CreateModelAsync(createElectricVehicleModelDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-models")]
        public async Task<ActionResult<ResponseDTO>> GetAllModels()
        {
            var response = await _electricVehicleModelService.GetAllModelsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-model-by-id/{modelId}")]
        public async Task<ActionResult<ResponseDTO>> GetModelById([FromRoute] Guid modelId)
        {
            var response = await _electricVehicleModelService.GetModelByIdAsync(modelId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-model-by-name/{modelName}")]
        public async Task<ActionResult<ResponseDTO>> GetModelByName([FromRoute] string modelName)
        {
            var response = await _electricVehicleModelService.GetModelByNameAsync(modelName);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-model/{modelId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateModel([FromRoute] Guid modelId, [FromBody] UpdateElectricVehicleModelDTO updateElectricVehicleModelDTO)
        {
            var response = await _electricVehicleModelService.UpdateModelAsync(modelId, updateElectricVehicleModelDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("delete-model/{modelId}")]
        public async Task<ActionResult<ResponseDTO>> DeleteModel([FromRoute] Guid modelId)
        {
            var response = await _electricVehicleModelService.DeleteModelAsync(modelId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
