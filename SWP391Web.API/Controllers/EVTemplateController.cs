using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EVTemplate;
using SWP391Web.Application.IServices;
using System.Threading.Tasks;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EVTemplateController : ControllerBase
    {
        public readonly IEVTemplateService _evTemplateService;

        public EVTemplateController(IEVTemplateService evTemplateService)
        {
            _evTemplateService = evTemplateService ?? throw new ArgumentNullException(nameof(evTemplateService));
        }

        [HttpPost("create-template-vehicles")]
        public async Task<ActionResult<ResponseDTO>> CreateEVTemplateAsync(CreateEVTemplateDTO createEVTemplateDTO)
        {
            var response = await _evTemplateService.CreateEVTemplateAsync(createEVTemplateDTO);
            return StatusCode(response.StatusCode,response);
        }

        [HttpGet("Get-all-template-vehicles")]
        public async Task<ActionResult<ResponseDTO>> GetAllEVTemplate()
        {
            var response = await _evTemplateService.GetAllVehicleTemplateAsync();
            return StatusCode(response.StatusCode,response);
        }

        [HttpGet("get-template-by-id/{EVTemplateId}")]
        public async Task<ActionResult<ResponseDTO>> GetEVTemplateById(Guid EVTemplateId)
        {
            var response = await _evTemplateService.GetVehicleTemplateByIdAsync(EVTemplateId);
            return StatusCode(response.StatusCode,response);
        }

        [HttpPut("update-template-vehicle/{EVTemplateId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateEVTemplate(Guid EVTemplateId ,UpdateEVTemplateDTO updateEVTemplateDTO)
        {
            var response = await _evTemplateService.UpdateEVTemplateAsync(EVTemplateId, updateEVTemplateDTO);
            return StatusCode(response.StatusCode,response);
        }

        [HttpDelete("delete-template/{EVTemplateId}")]
        public async Task<ActionResult<ResponseDTO>> DeleteEVTemplate(Guid EVTemplateId)
        {
            var response = await _evTemplateService.DeleteEVTemplateAsync(EVTemplateId);
            return StatusCode(response.StatusCode,response);
        }

        [HttpGet("get-template-by-version-and-color/{versionId}/{colorId}")]
        public async Task<ActionResult<ResponseDTO>> GetTemplesByVersionAndColor(Guid versionId, Guid colorId)
        {
            var response = await _evTemplateService.GetTemplatesByVersionAndColorAsync(versionId, colorId);
            return StatusCode(response.StatusCode,response);
        }
    }
}
