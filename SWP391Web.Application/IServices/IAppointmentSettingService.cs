using SWP391Web.Application.DTO.AppointmentSetting;
using SWP391Web.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IAppointmentSettingService
    {
        Task<ResponseDTO> CreateAppointmentAsync(CreateAppointmentDTO createAppointmentDTO);
        Task<ResponseDTO> DeleteAppointmentAsync(Guid appointmentId);
        Task<ResponseDTO> GetAllAppointmentAsync();
        Task<ResponseDTO> GetAppointmentByDealerIdAsync(Guid dealerId);
        Task<ResponseDTO> UpdateAppointmentAsync(Guid appointmentId ,UpdateAppointmentDTO updateAppointmentDTO);

    }
}
