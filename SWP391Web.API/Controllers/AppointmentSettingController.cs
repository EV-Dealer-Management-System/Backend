using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.AppointmentSetting;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentSettingController : ControllerBase
    {
        private readonly IAppointmentSettingService _appointmentSettingService;

        public AppointmentSettingController(IAppointmentSettingService appointmentSettingService)
        {
            _appointmentSettingService = appointmentSettingService ?? throw new ArgumentNullException(nameof(appointmentSettingService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _appointmentSettingService.GetAllAppointmentAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("get-appointment-by-dealer-id/{dealerId}")]
        public async Task<ActionResult<ResponseDTO>> GetByDealerIdAsync(Guid dealerId)
        {
            var result = await _appointmentSettingService.GetAppointmentByDealerIdAsync(dealerId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("create-appointment")]
        public async Task<ActionResult<ResponseDTO>> CreateAppointmentAsync([FromBody] CreateAppointmentDTO createAppointmentDTO)
        {
            var result = await _appointmentSettingService.CreateAppointmentAsync(createAppointmentDTO);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("update-appointment-by-id/{appointmentId}")]
        public async Task<IActionResult> Update([FromRoute]Guid appointmentId, [FromBody]UpdateAppointmentDTO updateAppointmentDTO)
        {
            var result = await _appointmentSettingService.UpdateAppointmentAsync(appointmentId,updateAppointmentDTO);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("delete-appointment/{appointmentId}")]
        public async Task<IActionResult> Delete(Guid appointmentId)
        {
            var result = await _appointmentSettingService.DeleteAppointmentAsync(appointmentId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
