using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.VehicleDelivery;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class VehicleDeliveryService : IVehicleDeliveryService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IBookingEVService _bookingEVService;

        public VehicleDeliveryService(IUnitOfWork unitOfWork, IMapper mapper, IBookingEVService bookingEVService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _bookingEVService = bookingEVService;
        }
        public async Task<ResponseDTO> GetAllVehicleDelivery(DeliveryStatus? status = null)
        {
            try
            {
                Func<IQueryable<VehicleDelivery>, IQueryable<VehicleDelivery>> includes = q =>
                q.Include(vd => vd.BookingEV)
                    .ThenInclude(b => b.Dealer);

                Expression<Func<VehicleDelivery, bool>>? filter = null;
                if (status.HasValue)
                {
                    filter = vd => vd.Status == status.Value;
                }

                var deliveries = await _unitOfWork.VehicleDeliveryRepository.GetAllAsync(
                    filter: filter,
                    includes: includes);

                var getDeliveries = _mapper.Map<List<GetVehicleDeliveryDTO>>(deliveries);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all deliveries successfully",
                    StatusCode = 200,
                    Result = getDeliveries
                };
                   
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleDeliveryById(Guid deliveryId , CancellationToken ct)
        {
            try
            {
                var delivery = await _unitOfWork.VehicleDeliveryRepository.GetVehicleDeliveryById(deliveryId,ct);
                if (delivery == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery not found",
                        StatusCode = 400,
                    };
                }

                var getDelivery = _mapper.Map<GetVehicleDeliveryDTO>(delivery);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get Delivery successfully",
                    StatusCode = 200,
                    Result = getDelivery
                };

            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> UpdateVehicleDeliveryStatus(ClaimsPrincipal user, Guid deliveryId, DeliveryStatus newStatus, CancellationToken ct, string? reason = null)
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

                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (role != StaticUserRole.Admin && role != StaticUserRole.EVMStaff)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only Admin or EVM Staff can update delivery status",
                        StatusCode = 403
                    };
                }

                var delivery = await _unitOfWork.VehicleDeliveryRepository.GetVehicleDeliveryById(deliveryId, ct);
                if (delivery == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery not found",
                        StatusCode = 404
                    };
                }

                if (delivery.Status == DeliveryStatus.Preparing && newStatus != DeliveryStatus.Packing)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery can only move from Preparing to Packing",
                        StatusCode = 400
                    };
                }

                if (delivery.Status == DeliveryStatus.Packing && newStatus != DeliveryStatus.InTransit)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Delivery can only move from Packing to InTransit",
                        StatusCode = 400
                    };
                }

                if (delivery.Status == DeliveryStatus.InTransit &&
                    !(newStatus == DeliveryStatus.InTransit 
                    || newStatus == DeliveryStatus.Accident 
                    || newStatus == DeliveryStatus.Delayed
                    || newStatus == DeliveryStatus.Arrived))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "From InTransit, status can only change to InTransit, Arrived, Accident or Delayed",
                        StatusCode = 400
                    };
                }

                if (delivery.Status == DeliveryStatus.Arrived &&
                    newStatus != DeliveryStatus.Confirmed)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "After Arrived, delivery can only move to Confirmed",
                        StatusCode = 400
                    };
                }

                switch (newStatus)
                {
                    case DeliveryStatus.Preparing:
                        delivery.Description = "Chuẩn bị xe để vận chuyển";
                        break;
                    case DeliveryStatus.Packing:
                        delivery.Description = "Xe đang được đóng gói";
                        break;
                    case DeliveryStatus.InTransit:
                        delivery.Description = "Xe đang trên đường vận chuyển";
                        break;
                    case DeliveryStatus.Arrived:
                        delivery.Description = "Xe đã đến đại lý";
                        break;
                    case DeliveryStatus.Confirmed:
                        delivery.Description = "Giao nhận hoàn tất";
                        break;
                    case DeliveryStatus.Delayed:
                    case DeliveryStatus.Accident:
                        if (string.IsNullOrWhiteSpace(reason))
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Bạn phải nhập lý do khi có tai nạn hoặc delay",
                                StatusCode = 400
                            };
                        }
                        delivery.Description = reason;
                        break;
                }

                delivery.Status = newStatus;
                delivery.UpdateAt = DateTime.UtcNow;
                _unitOfWork.VehicleDeliveryRepository.Update(delivery);

                if (newStatus == DeliveryStatus.InTransit)
                {
                    foreach (var dt in delivery.BookingEV.BookingEVDetails)
                    {
                        var vehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetBookedVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                        foreach (var ev in vehicles.Take(dt.Quantity))
                        {
                            ev.Status = ElectricVehicleStatus.InTransit;
                            _unitOfWork.ElectricVehicleRepository.Update(ev);
                        }
                    }
                }
                else if (newStatus == DeliveryStatus.Accident)
                {
                    await UpdateStatusAccidentAsync(delivery, reason!);
                }
                else if (newStatus == DeliveryStatus.Confirmed)
                {
                    await _bookingEVService.ConfirmBookingDeliveryAsync(user, delivery.BookingEV.Id, ct);
                }

                await _unitOfWork.SaveAsync();

                var getDelivery = _mapper.Map<GetVehicleDeliveryDTO>(delivery);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Delivery status updated to {newStatus}",
                    StatusCode = 200,
                    Result = getDelivery
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

        private async Task<ResponseDTO> UpdateStatusAccidentAsync(VehicleDelivery delivery, string reason)
        {
            delivery.Description = reason;

            foreach (var dt in delivery.BookingEV.BookingEVDetails)
            {
                var vehicles = await _unitOfWork.ElectricVehicleRepository
                    .GetBookedVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                foreach (var ev in vehicles.Take(dt.Quantity))
                {
                    ev.Status = ElectricVehicleStatus.Maintenance;
                    _unitOfWork.ElectricVehicleRepository.Update(ev);
                }
            }

            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Update status successfully",
                StatusCode = 200,
            };
        }
    }
}
