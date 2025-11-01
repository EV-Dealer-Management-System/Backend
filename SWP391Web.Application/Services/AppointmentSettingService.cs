using AutoMapper;
using SWP391Web.Application.DTO.AppointmentSetting;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class AppointmentSettingService : IAppointmentSettingService
        {
            public readonly IUnitOfWork _unitOfWork;
            public readonly IMapper _mapper;
            public AppointmentSettingService(IUnitOfWork unitOfWork, IMapper mapper)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }
        public async Task<ResponseDTO> CreateAppointmentAsync(ClaimsPrincipal user, CreateAppointSettingDTO createAppointmentDTO)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }
                if (createAppointmentDTO.OpenTime >= createAppointmentDTO.CloseTime)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Open time must be earlier than close time",
                        StatusCode = 400
                    };
                }

                var totalWorkHours = createAppointmentDTO.CloseTime - createAppointmentDTO.OpenTime;
                if(totalWorkHours.TotalHours >= 24 || totalWorkHours.TotalHours <=0)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Total working hours must be between 0 and 24 hours",
                        StatusCode = 400
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User role not found",
                        StatusCode = 404
                    };
                }

                if(role == StaticUserRole.Admin)
                {
                    var dast = await _unitOfWork.AppointmentSettingRepository.GetDefaultAsync();
                    if (dast != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Default appointment setting already exists",
                            StatusCode = 400
                        };
                    }

                    var defaultAppointmentSetting = new AppointmentSetting
                    {
                        ManagerId = userId,
                        DealerId = null,
                        AllowOverlappingAppointments = createAppointmentDTO.AllowOverlappingAppointments,
                        MaxConcurrentAppointments = createAppointmentDTO.MaxConcurrentAppointments,
                        OpenTime = createAppointmentDTO.OpenTime,
                        CloseTime = createAppointmentDTO.CloseTime,
                        MinIntervalBetweenAppointments = createAppointmentDTO.MinIntervalBetweenAppointments,
                        BreakTimeBetweenAppointments = createAppointmentDTO.BreakTimeBetweenAppointments,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.AppointmentSettingRepository.AddAsync(defaultAppointmentSetting, CancellationToken.None);
                    await _unitOfWork.SaveAsync();

                    var getDefaultAppointmentSetting = _mapper.Map<GetAppointSettingDTO>(defaultAppointmentSetting);
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Default appointment setting created successfully",
                        StatusCode = 201,
                        Result = getDefaultAppointmentSetting
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var ast = await _unitOfWork.AppointmentSettingRepository.GetByDealerIdAsync(dealer.Id);
                if (ast != null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Appointment setting already exists for this dealer",
                        StatusCode = 400
                    };
                }

                var appointmentSetting = new AppointmentSetting
                {
                    ManagerId = userId,
                    DealerId = dealer.Id,
                    AllowOverlappingAppointments = createAppointmentDTO.AllowOverlappingAppointments,
                    MaxConcurrentAppointments = createAppointmentDTO.MaxConcurrentAppointments,
                    OpenTime = createAppointmentDTO.OpenTime,
                    CloseTime = createAppointmentDTO.CloseTime,
                    MinIntervalBetweenAppointments = createAppointmentDTO.MinIntervalBetweenAppointments,
                    BreakTimeBetweenAppointments = createAppointmentDTO.BreakTimeBetweenAppointments,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.AppointmentSettingRepository.AddAsync(appointmentSetting,CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var getAppointmentSetting = _mapper.Map<GetAppointSettingDTO>(appointmentSetting);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment setting created successfully",
                    StatusCode = 201,
                    Result = getAppointmentSetting
                };
            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public Task<ResponseDTO> DeleteAppointmentAsync(Guid appointmentId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDTO> GenerateTimeSlotAsync(ClaimsPrincipal user, DateTime? targetDate = null)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if(dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var appointmentSetting = await _unitOfWork.AppointmentSettingRepository.GetByDealerIdAsync(dealer.Id);
                if(appointmentSetting == null)
                {
                    appointmentSetting = await _unitOfWork.AppointmentSettingRepository.GetDefaultAsync();
                    if (appointmentSetting == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Default appointment setting not found",
                            StatusCode = 404
                        };
                    }
                    if(appointmentSetting == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Appointment setting not found",
                            StatusCode = 404
                        };
                    }
                }

                // Take today dealer's appointment
                var date = targetDate?.Date ?? DateTime.UtcNow.Date;
                var appointments = await _unitOfWork.AppointmentRepository
                    .GetByDealerIdAndDateAsync(dealer.Id,date);

                var slots = new List<GetAppointmentSlotDTO>();
                var currentTime = appointmentSetting.OpenTime;
                var interval = TimeSpan.FromMinutes(appointmentSetting.MinIntervalBetweenAppointments);
                var breakTime = TimeSpan.FromMinutes(appointmentSetting.BreakTimeBetweenAppointments);
                var  end = appointmentSetting.CloseTime;
                bool skipBreak = false; // Flag to skip adding break time after the last slot

                while(currentTime + interval <= end)
                {
                    var slotStartTime = currentTime;
                    var slotEndTime = currentTime + interval;
                    var overlappingAppointments = appointments
                        .Where(a => 
                            (a.StartTime.TimeOfDay < slotEndTime) && 
                            (a.EndTime.TimeOfDay > currentTime) &&
                            a.Status == AppointmentStatus.Active)
                        .ToList();
                    bool isAvailable;
                    if(appointmentSetting.AllowOverlappingAppointments)
                    {
                        isAvailable = overlappingAppointments.Count < appointmentSetting.MaxConcurrentAppointments;
                    }
                    else
                    {
                        isAvailable = overlappingAppointments.Count == 0;
                    }
                    slots.Add(new GetAppointmentSlotDTO
                    {
                        OpenTime = currentTime,
                        CloseTime = slotEndTime,
                        IsAvailable = isAvailable
                    });
                    currentTime += interval;
                    // Add break time if there are more slots to process
                    if (!skipBreak && currentTime + interval <= end)
                    {
                        currentTime += breakTime;
                    }
                }


                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment slots generated successfully",
                    StatusCode = 200,
                    Result = slots
                };

            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }

        }

        public async Task<ResponseDTO> GetAppointmentByIdAsync(Guid appointmentId)
        {
            try
            {
                var appointmentSetting = await _unitOfWork.AppointmentSettingRepository.GetById(appointmentId);
                if (appointmentSetting == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Appointment setting not found",
                        StatusCode = 404
                    };
                }
                var getAppointmentSetting = _mapper.Map<GetAppointSettingDTO>(appointmentSetting);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment setting retrieved successfully",
                    StatusCode = 200,
                    Result = getAppointmentSetting
                };
            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetCurrentUserSettingAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 400
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found"
                    };
                }


                var appointmentSetting = await _unitOfWork.AppointmentSettingRepository.GetByDealerIdAsync(dealer.Id);

                var getAppointmentSetting = _mapper.Map<AppointmentSetting>(appointmentSetting);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get settings successfully",
                    StatusCode = 200,
                    Result = getAppointmentSetting
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        //public async Task<ResponseDTO> GetAllAppointmentAsync(ClaimsPrincipal user)
        //{
        //    try
        //    {
        //        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        //        if (userId == null || role == null)
        //        {
        //            return new ResponseDTO
        //            {
        //                IsSuccess = false,
        //                Message = "Invalid user claims",
        //                StatusCode = 401
        //            };
        //        }

        //        if (role == StaticUserRole.Admin)
        //        {
        //            // Admin can view all appointment settings
        //            var allSettings = await _unitOfWork.AppointmentSettingRepository.GetAllAsync();
        //            if (allSettings == null || !allSettings.Any())
        //            {
        //                return new ResponseDTO
        //                {
        //                    IsSuccess = false,
        //                    Message = "No appointment settings found",
        //                    StatusCode = 404
        //                };
        //            }

        //            var mapped = _mapper.Map<List<GetAppointSettingDTO>>(allSettings);
        //            return new ResponseDTO
        //            {
        //                IsSuccess = true,
        //                Message = "All appointment settings retrieved successfully",
        //                StatusCode = 200,
        //                Result = mapped
        //            };
        //        }

        //        if (role == StaticUserRole.DealerManager)
        //        {
        //            //Dealer Manager can view their dealer's appointment settings
        //            var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, CancellationToken.None);
        //            if (dealer == null)
        //            {
        //                return new ResponseDTO
        //                {
        //                    IsSuccess = false,
        //                    Message = "Dealer not found for this manager",
        //                    StatusCode = 404
        //                };
        //            }

        //            var dealerSetting = await _unitOfWork.AppointmentSettingRepository.GetByDealerIdAsync(dealer.Id);
        //            if (dealerSetting == null)
        //            {
        //                return new ResponseDTO
        //                {
        //                    IsSuccess = false,
        //                    Message = "Appointment setting not found for this dealer",
        //                    StatusCode = 404
        //                };
        //            }

        //            var mapped = _mapper.Map<GetAppointSettingDTO>(dealerSetting);
        //            return new ResponseDTO
        //            {
        //                IsSuccess = true,
        //                Message = "Dealer appointment setting retrieved successfully",
        //                StatusCode = 200,
        //                Result = mapped
        //            };
        //        }

        //        // Other roles are not authorized to view appointment settings
        //        return new ResponseDTO
        //        {
        //            IsSuccess = false,
        //            Message = "You do not have permission to view appointment settings",
        //            StatusCode = 403
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseDTO
        //        {
        //            IsSuccess = false,
        //            Message = ex.Message,
        //            StatusCode = 500
        //        };
        //    }
        //}

        public async Task<ResponseDTO> UpdateAppointmentAsync(ClaimsPrincipal user ,Guid appointmentId, UpdateAppointSettingDTO updateAppointmentDTO)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User role not found",
                        StatusCode = 404
                    };
                }

                var appointmentSetting = await _unitOfWork.AppointmentSettingRepository.GetById(appointmentId);
                if (appointmentSetting == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Appointment setting not found",
                        StatusCode = 404
                    };
                }

                if (role == StaticUserRole.Admin)
                {
                    // Admins can only update default appointment settings
                    if (appointmentSetting.DealerId != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Admins can only update default appointment settings.",
                            StatusCode = 403
                        };
                    }
                }
                else if (role == StaticUserRole.DealerManager)
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, CancellationToken.None);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }

                    // Dealer Managers can only update their own dealer's appointment settings
                    if (appointmentSetting.DealerId != dealer.Id)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "You do not have permission to update this appointment setting.",
                            StatusCode = 403
                        };
                    }
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Unauthorized role.",
                        StatusCode = 403
                    };
                }


                if (updateAppointmentDTO.OpenTime.HasValue && updateAppointmentDTO.CloseTime.HasValue &&
                    updateAppointmentDTO.OpenTime >= updateAppointmentDTO.CloseTime)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Open time must be earlier than close time",
                        StatusCode = 400
                    };



                if (updateAppointmentDTO.MaxConcurrentAppointments.HasValue && updateAppointmentDTO.MaxConcurrentAppointments <= 0)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Maximum concurrent appointments must be greater than 0",
                        StatusCode = 400
                    };

                if (updateAppointmentDTO.MinIntervalBetweenAppointments.HasValue && updateAppointmentDTO.MinIntervalBetweenAppointments <= 0)
                    return new ResponseDTO
                    { 
                        IsSuccess = false, 
                        Message = "Minimum interval must be greater than 0 minutes", 
                        StatusCode = 400 
                    };

                if (updateAppointmentDTO.AllowOverlappingAppointments.HasValue)
                {
                    appointmentSetting.AllowOverlappingAppointments = updateAppointmentDTO.AllowOverlappingAppointments.Value;
                }
                if (updateAppointmentDTO.MaxConcurrentAppointments.HasValue)
                {
                    appointmentSetting.MaxConcurrentAppointments = updateAppointmentDTO.MaxConcurrentAppointments.Value;
                }
                if (updateAppointmentDTO.OpenTime.HasValue)
                {
                    appointmentSetting.OpenTime = updateAppointmentDTO.OpenTime.Value;
                }
                if (updateAppointmentDTO.CloseTime.HasValue)
                {
                    appointmentSetting.CloseTime = updateAppointmentDTO.CloseTime.Value;
                }
                if (updateAppointmentDTO.MinIntervalBetweenAppointments.HasValue)
                {
                    appointmentSetting.MinIntervalBetweenAppointments = updateAppointmentDTO.MinIntervalBetweenAppointments.Value;
                }

                
                _unitOfWork.AppointmentSettingRepository.Update(appointmentSetting);
                await _unitOfWork.SaveAsync();

                var getAppointmentSetting = _mapper.Map<GetAppointSettingDTO>(appointmentSetting);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment setting updated successfully",
                    StatusCode = 200,
                    Result = getAppointmentSetting

                };

            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }
    }
}
