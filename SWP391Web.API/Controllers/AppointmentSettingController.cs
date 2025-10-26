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

        [HttpPost("create-appointment-setting")]
        public async Task<ActionResult<ResponseDTO>> CreateAppointmentAsync([FromBody] CreateAppointSettingDTO createAppointmentDTO)
        {
            var response = await _appointmentSettingService.CreateAppointmentAsync(User, createAppointmentDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-all-appointment-setting")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _appointmentSettingService.GetAllAppointmentAsync(User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-appointment-setting-by-dealer-id/{dealerId}")]
        public async Task<ActionResult<ResponseDTO>> GetByDealerIdAsync()
        {
            var response = await _appointmentSettingService.GetAppointmentByDealerIdAsync(User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("update-appointment-setting-by-id/{appointmentId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateAppointmentSettingAsync([FromRoute]Guid appointmentId, [FromBody]UpdateAppointSettingDTO updateAppointmentDTO)
        {
            var response = await _appointmentSettingService.UpdateAppointmentAsync(User,appointmentId,updateAppointmentDTO);
            return StatusCode(response.StatusCode, response);
        }

        //[HttpDelete("delete-appointment/{appointmentId}")]
        //public async Task<IActionResult> Delete(Guid appointmentId)
        //{
        //    var result = await _appointmentSettingService.DeleteAppointmentAsync(appointmentId);
        //    return StatusCode(result.StatusCode, result);
        //}
    }
}
