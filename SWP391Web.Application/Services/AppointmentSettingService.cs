using SWP391Web.Application.DTO.AppointmentSetting;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
        public class AppointmentSettingService : IAppointmentSettingService
        {
            private static readonly List<AppointmentSetting> _settings = new()
            {
                new AppointmentSetting
                {
                    Id = Guid.NewGuid(),
                    ManagerId = "admin",
                    DealerId = null,
                    AllowOverlappingAppointments = false,
                    MaxConcurrentAppointments = 1,
                    OpenTime = new TimeSpan(8, 0, 0),
                    CloseTime = new TimeSpan(17, 0, 0),
                    MinIntervalBetweenAppointments = 30,
                    CreatedAt = DateTime.UtcNow
                }
            };
            public Task<ResponseDTO> CreateAppointmentAsync(CreateAppointmentDTO createAppointmentDTO)
            {
                var entity = new AppointmentSetting
                {
                    Id = Guid.NewGuid(),
                    ManagerId = createAppointmentDTO.ManagerId,
                    DealerId = createAppointmentDTO.DealerId,
                    AllowOverlappingAppointments = createAppointmentDTO.AllowOverlappingAppointments,
                    MaxConcurrentAppointments = createAppointmentDTO.MaxConcurrentAppointments,
                    OpenTime = createAppointmentDTO.OpenTime,
                    CloseTime = createAppointmentDTO.CloseTime,
                    MinIntervalBetweenAppointments = createAppointmentDTO.MinIntervalBetweenAppointments,
                    CreatedAt = DateTime.UtcNow
                };

                _settings.Add(entity);

                return Task.FromResult(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment setting created successfully",
                    StatusCode = 201,
                    Result = entity
                });
        }

            public Task<ResponseDTO> DeleteAppointmentAsync(Guid appointmentId)
            {
                var setting = _settings.FirstOrDefault(s => s.Id == appointmentId);
                if (setting == null)
                {
                    return Task.FromResult(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Appointment setting not found",
                        StatusCode = 404
                    });
                }

                _settings.Remove(setting);

                return Task.FromResult(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment setting deleted successfully",
                    StatusCode = 200
                });
        }

            public Task<ResponseDTO> GetAllAppointmentAsync()
            {
                var result =  _settings.Select(s => new
                {
                    s.Id,
                    s.ManagerId,
                    s.DealerId,
                    s.AllowOverlappingAppointments,
                    s.MaxConcurrentAppointments,
                    s.OpenTime,
                    s.CloseTime,
                    s.MinIntervalBetweenAppointments,
                    s.CreatedAt
                }).ToList();

                return Task.FromResult(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "All appointment settings retrieved",
                    StatusCode = 200,
                    Result = result
                });
        }

            public Task<ResponseDTO> GetAppointmentByDealerIdAsync(Guid dealerId)
            {
                var setting = _settings.FirstOrDefault(s => s.DealerId == dealerId)
                          ?? _settings.FirstOrDefault(s => s.DealerId == null);

                if (setting == null)
                {
                    return Task.FromResult(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No appointment setting found",
                        StatusCode = 404
                    });
                }

                return Task.FromResult(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment setting retrieved",
                    StatusCode = 200,
                    Result = setting
                });
        }

            public Task<ResponseDTO> UpdateAppointmentAsync(Guid appointmentId,UpdateAppointmentDTO updateAppointmentDTO)
            {
                var setting = _settings.FirstOrDefault(s => s.Id == appointmentId);
                if (setting == null)
                {
                    return Task.FromResult(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Appointment setting not found",
                        StatusCode = 404
                    });
                }

                if (updateAppointmentDTO.AllowOverlappingAppointments.HasValue)
                    setting.AllowOverlappingAppointments = updateAppointmentDTO.AllowOverlappingAppointments.Value;

                if (updateAppointmentDTO.MaxConcurrentAppointments.HasValue)
                    setting.MaxConcurrentAppointments = updateAppointmentDTO.MaxConcurrentAppointments.Value;

                if (updateAppointmentDTO.OpenTime.HasValue)
                    setting.OpenTime = updateAppointmentDTO.OpenTime.Value;

                if (updateAppointmentDTO.CloseTime.HasValue)
                    setting.CloseTime = updateAppointmentDTO.CloseTime.Value;

                if (updateAppointmentDTO.MinIntervalBetweenAppointments.HasValue)
                    setting.MinIntervalBetweenAppointments = updateAppointmentDTO.MinIntervalBetweenAppointments.Value;

                return Task.FromResult(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment setting updated successfully",
                    StatusCode = 200,
                    Result = setting
                });
            }
        }
}
