using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DepositSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IDepositSettingService
    {
        Task<ResponseDTO> CreateUpdateDepositSetting(ClaimsPrincipal userClaim, decimal depositPercentage, CancellationToken ct);
        Task<ResponseDTO> GetDepositSetting(ClaimsPrincipal userClaim, CancellationToken ct);
        Task<ResponseDTO> UpdateAllSettings(ClaimsPrincipal userClaim, UpdateAllDepositSettingsDTO settingsDTO, CancellationToken ct);
    }
}
