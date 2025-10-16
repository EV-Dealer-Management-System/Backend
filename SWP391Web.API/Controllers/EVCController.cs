using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EVC;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EVCController : ControllerBase
    {
        private readonly IEVCService _evcService;
        public EVCController(IEVCService evcService)
        {
            _evcService = evcService ?? throw new ArgumentNullException(nameof(evcService));
        }

        [HttpPost]
        [Route("create-evm-staff")]
        public async Task<ActionResult<ResponseDTO>> CreateEVMStaff([FromBody] CreateEVMStaffDTO createEVMStaffDTO)
        {
            var response = await _evcService.CreateEVMStaff(createEVMStaffDTO);
            return StatusCode(response.StatusCode, response);
        }
    }
}
