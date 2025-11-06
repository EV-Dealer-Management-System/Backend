using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DepositSetting;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepositSettingController : ControllerBase
    {
        private readonly IDepositSettingService _depositSettingService;
        public DepositSettingController(IDepositSettingService depositSettingService)
        {
            _depositSettingService = depositSettingService;
        }
        [HttpPost]
        [Route("create-update-deposit-setting-dealer")]
        [Authorize(Roles = StaticUserRole.DealerManager)]
        public async Task<ActionResult<ResponseDTO>> CreateUpdateDepositSetting([FromQuery] decimal depositPercentage, CancellationToken ct)
        {
            var response = await _depositSettingService.CreateUpdateDepositSetting(User, depositPercentage, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-deposit-setting")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO>> GetDepositSetting(CancellationToken ct)
        {
            var response = await _depositSettingService.GetDepositSetting(User, ct);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("update-all-deposit-settings")]
        [Authorize(Roles = StaticUserRole.Admin)]
        public async Task<ActionResult<ResponseDTO>> UpdateAllSettings([FromBody] UpdateAllDepositSettingsDTO settingsDTO, CancellationToken ct)
        {
            var response = await _depositSettingService.UpdateAllSettings(User, settingsDTO, ct);
            return StatusCode(response.StatusCode, response);
        }
    }
}
