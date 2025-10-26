using AutoMapper;
using SWP391Web.Application.DTO.Appointment;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
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
    public class AppointmentService : IAppointmentService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateAppointmentAsync(ClaimsPrincipal user, CreateAppointmentDTO createAppointmentDTO)
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
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var appointmentSetting = await _unitOfWork.AppointmentSettingRepository.GetByDealerIdAsync(dealer.Id);
                if (appointmentSetting == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Appointment setting not found",
                        StatusCode = 404
                    };
                }

                // Convert UTC -> Dealer local time
                var dealerTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // UTC+7
                var startLocal = TimeZoneInfo.ConvertTimeFromUtc(createAppointmentDTO.StartTime, dealerTimeZone);
                var endLocal = TimeZoneInfo.ConvertTimeFromUtc(createAppointmentDTO.EndTime, dealerTimeZone);
                // Check time is true
                if (startLocal.TimeOfDay < appointmentSetting.OpenTime || endLocal.TimeOfDay > appointmentSetting.CloseTime)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Appointment time is outside of allowed hours",
                        StatusCode = 400
                    };

                if (createAppointmentDTO.StartTime >= createAppointmentDTO.EndTime)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid appointment time range",
                        StatusCode = 400
                    };

                var minInterval = TimeSpan.FromMinutes(appointmentSetting.MinIntervalBetweenAppointments); // each slot own how many mins
                var slots = new List<(DateTime Start, DateTime End)>(); // List to check overlapping
                //Check time in UTC to check
                var currentUtc = createAppointmentDTO.StartTime;
                var endUtc = createAppointmentDTO.EndTime;

                // divine into many slot base on minInterval
                while (currentUtc < endUtc)
                {
                    //Convert UTC -> Local
                    var currentLocal = TimeZoneInfo.ConvertTimeFromUtc(currentUtc, dealerTimeZone);
                    var openLocal = currentLocal.Date + appointmentSetting.OpenTime;
                    var closeLocal = currentLocal.Date + appointmentSetting.CloseTime;

                    //Findout which slot start and end
                    var slotStartLocal = currentLocal < openLocal ? openLocal : currentLocal;
                    var slotEndLocal = slotStartLocal + minInterval;
                    // if over CloseTime -> = CloseTime
                    if (slotEndLocal > closeLocal) slotEndLocal = closeLocal;
                    // if over Endtime -> = Endtime
                    if (slotEndLocal > TimeZoneInfo.ConvertTimeFromUtc(endUtc, dealerTimeZone))
                        slotEndLocal = TimeZoneInfo.ConvertTimeFromUtc(endUtc, dealerTimeZone);

                    if (slotStartLocal < slotEndLocal)
                    {
                        // Convert Local -> UTC
                        var slotStartUtc = TimeZoneInfo.ConvertTimeToUtc(slotStartLocal, dealerTimeZone);
                        var slotEndUtc = TimeZoneInfo.ConvertTimeToUtc(slotEndLocal, dealerTimeZone);
                        slots.Add((slotStartUtc, slotEndUtc));
                    }

                    currentUtc = TimeZoneInfo.ConvertTimeToUtc(slotEndLocal, dealerTimeZone);
                }


                // Check overlapping
                foreach (var s in slots)
                {
                    var count = await _unitOfWork.AppointmentRepository.CountOverLappingAsync(dealer.Id, s.Start, s.End);
                    if (!appointmentSetting.AllowOverlappingAppointments && count >= 1) // Only 1
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "No overlapping so only 1 appointment can be created",
                            StatusCode = 400
                        };
                    }

                    if (appointmentSetting.AllowOverlappingAppointments && count >= appointmentSetting.MaxConcurrentAppointments)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Over maxconcurrentsetting",
                            StatusCode = 400
                        };
                    }

                }

                //Create
                var appointment = new Appointment
                {
                    DealerId = dealer.Id,
                    CustomerId = createAppointmentDTO.CustomerId,
                    EVTemplateId = createAppointmentDTO.EVTemplateId,
                    StartTime = createAppointmentDTO.StartTime,
                    EndTime = createAppointmentDTO.EndTime,
                    Status = AppointmentStatus.Active,
                    Note = createAppointmentDTO.Note,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.AppointmentRepository.AddAsync(appointment, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var getAppointmentDTO = _mapper.Map<GetAppointmentDTO>(appointment);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment created successfully",
                    StatusCode = 201,
                    Result = getAppointmentDTO
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

        public async Task<ResponseDTO> GetAllAppointmentsAsync(ClaimsPrincipal user)
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
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var appointments = await _unitOfWork.AppointmentRepository.GetAllByDealerIdAsync(dealer.Id);
                if (appointments == null || !appointments.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No appointments found",
                        StatusCode = 404
                    };
                }

                var getAppointments = _mapper.Map<List<GetAppointmentDTO>>(appointments);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointments retrieved successfully",
                    StatusCode = 200,
                    Result = getAppointments
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
        public async Task<ResponseDTO> GetAppointmentsByCustomerIdAsync(ClaimsPrincipal user, Guid customerId)
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
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer not found",
                        StatusCode = 404
                    };
                }
                // take customer appointments
                var appointments = await _unitOfWork.AppointmentRepository.GetByCustomerIdAsync(customerId);
                if (appointments == null || !appointments.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No appointments found",
                        StatusCode = 404
                    };
                }

                var getAppointments = _mapper.Map<List<GetAppointmentDTO>>(appointments);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointments retrieved successfully",
                    StatusCode = 200,
                    Result = getAppointments
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

        public async Task<ResponseDTO> UpdateAppointmentStatusAsync(ClaimsPrincipal user, Guid appointmentId, AppointmentStatus newStatus)
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
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var appointments = await _unitOfWork.AppointmentRepository.GetByIdAsync(appointmentId);
                if (appointments == null || appointments.DealerId != dealer.Id)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No appointments found for this dealer",
                        StatusCode = 404
                    };
                }

                var appointmentSetting = await _unitOfWork.AppointmentSettingRepository.GetByDealerIdAsync(dealer.Id);
                if (appointmentSetting == null)
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
                }
                if (newStatus != AppointmentStatus.Completed && newStatus != AppointmentStatus.Canceled)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid status update. Can only update to Complete or Cancelled.",
                        StatusCode = 400
                    };
                }

                if (newStatus == AppointmentStatus.Canceled)
                {
                    if (!appointmentSetting.AllowOverlappingAppointments)
                    {
                        var overlappingCount = await _unitOfWork.AppointmentRepository
                            .CountOverLappingAsync(dealer.Id, appointments.StartTime, appointments.EndTime);
                        if (overlappingCount > 0)
                        {
                            // There are overlapping appointments just minus this one
                        }
                    }

                }

                //if status is completed, no need to check overlapping

                appointments.Status = newStatus;
                _unitOfWork.AppointmentRepository.Update(appointments);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Appointment status updated successfully",
                    StatusCode = 200

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
    }
}
