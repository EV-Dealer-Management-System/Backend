using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EVCInventory;
using SWP391Web.Application.IServices;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EVCInventoryController : ControllerBase
    {
        public readonly IEVCInventoryService _evcInventoryService;
        public EVCInventoryController(IEVCInventoryService evcInventoryService)
        {
            _evcInventoryService = evcInventoryService ?? throw new ArgumentNullException(nameof(evcInventoryService));
        }

        [HttpPost("create-evcinventory")]
        public async Task<ActionResult<ResponseDTO>> CreateEVCInventoryAsync(CreateEVCInventoryDTO createEVCInventoryDTO)
        {
            var response = await _evcInventoryService.CreateEVCInventoryAsync(createEVCInventoryDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-evcinventory-by-id/{id}")]
        public async Task<ActionResult<ResponseDTO>> GetEVCInventoryByIdAsync(Guid id)
        {
            var response = await _evcInventoryService.GetEVCInventoryByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-evcinventories")]
        public async Task<ActionResult<ResponseDTO>> GetAllEVCInventoriesAsync()
        {
            var response = await _evcInventoryService.GetAllEVCInventoriesAsync();
            return StatusCode(response.StatusCode, response);
        }
        //[HttpPut("update-evcinventory/{id}")]
        //public async Task<ActionResult<ResponseDTO>> UpdateEVCInventoryAsync(Guid id, UpdateEVCInventory updateEVCInventory)
        //{
        //    var response = await _evcInventoryService.UpdateEVCInventoryAsync(id, updateEVCInventory);
        //    return StatusCode(response.StatusCode, response);
        //}
        [HttpDelete("delete-evcinventory/{id}")]
        public async Task<ActionResult<ResponseDTO>> DeleteEVCInventoryAsync(Guid id)
        {
            var response = await _evcInventoryService.DeleteEVCInventoryAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }

}
