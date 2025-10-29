using SWP391Web.Application.DTO.AppointmentSetting;
using SWP391Web.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IAppointmentSettingService
    {
        Task<ResponseDTO> CreateAppointmentAsync(ClaimsPrincipal user, CreateAppointSettingDTO createAppointmentDTO);
        Task<ResponseDTO> DeleteAppointmentAsync(Guid appointmentId);
        Task<ResponseDTO> GetAppointmentByIdAsync(Guid appointmentId);
        Task<ResponseDTO> UpdateAppointmentAsync(ClaimsPrincipal user,Guid appointmentId ,UpdateAppointSettingDTO updateAppointmentDTO);
        Task<ResponseDTO> GenerateTimeSlotAsync(ClaimsPrincipal user , DateTime? targetDate = null);

    }
}
