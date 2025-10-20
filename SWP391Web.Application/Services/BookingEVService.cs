using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using SWP391Web.Application.DTO.BookingEVDetail;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using SWP391Web.Infrastructure.Repository;
using SWP391Web.Infrastructure.SignlR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Outline;

namespace SWP391Web.Application.Services
{
    public class BookingEVService : IBookingEVService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public BookingEVService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task<ResponseDTO> CreateBookingEVAsync(ClaimsPrincipal user, CreateBookingEVDTO createBookingEVDTO)
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
                        StatusCode = 404,
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, CancellationToken.None);

                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404,
                    };
                }

                var bookingEV = new BookingEV
                {
                    DealerId = dealer.Id,
                    Note = createBookingEVDTO.Note,
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Pending,
                    CreatedBy = dealer.Name,
                    TotalQuantity = createBookingEVDTO.BookingDetails.Sum(d => d.Quantity),
                };
                bookingEV.BookingEVDetails = createBookingEVDTO.BookingDetails.Select(detail => new BookingEVDetail
                {
                    VersionId = detail.VersionId,
                    ColorId = detail.ColorId,
                    Quantity = detail.Quantity,
                }).ToList();
                // change status to pending
                foreach(var dt in bookingEV.BookingEVDetails)
                {
                    var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByIdsAsync(dt.VersionId);
                    if (version == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " Version not found ",
                            StatusCode = 404
                        };
                    }

                    var availableVehicles = (await _unitOfWork.ElectricVehicleRepository
                        .GetAvailableVehicleByModelVersionColorAsync(version.ModelId, dt.VersionId, dt.ColorId))
                        .Where(ev => ev.Warehouse.WarehouseType == WarehouseType.EVInventory)
                        .ToList();
                    if(availableVehicles == null || !availableVehicles.Any())
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " No available vehicles in EVM warehouse",
                            StatusCode = 404
                        };
                    }

                    if(availableVehicles.Count() < dt.Quantity)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " Not enough vehicles for booking",
                            StatusCode = 400
                        };
                    }

                    var selectedVehicles = availableVehicles
                        .OrderBy(ev => ev.ImportDate)
                        .Take(dt.Quantity)
                        .ToList();

                    foreach( var ev in selectedVehicles)
                    {
                        ev.Status = ElectricVehicleStatus.Pending;
                        _unitOfWork.ElectricVehicleRepository.Update(ev);
                    }
                }

                await _unitOfWork.BookingEVRepository.AddAsync(bookingEV, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var bookingWithDetails = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingEV.Id);

                var getBookingEV = _mapper.Map<GetBookingEVDTO>(bookingWithDetails);

                var versionId = createBookingEVDTO.BookingDetails.First().VersionId;
                var colorId = createBookingEVDTO.BookingDetails.First().ColorId;
                var quantity = await _unitOfWork.ElectricVehicleRepository.GetAvailableQuantityByVersionColorAsync(versionId, colorId);
                await UpdateQuantityRealTime(versionId, colorId, quantity);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Booking created successfully",
                    StatusCode = 200,
                    Result = getBookingEV
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }

        }

        public async Task<ResponseDTO> GetAllBookingEVsAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if(userId == null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                var bookingEVs = new List<BookingEV>();
                if (role == StaticUserRole.Admin || role == StaticUserRole.EVMStaff)
                {
                     bookingEVs = (await _unitOfWork.BookingEVRepository.GetAllBookingWithDetailAsync()).ToList();
                }
                else
                {
                    var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, CancellationToken.None);

                    if (dealer == null)
                    {
                        return new ResponseDTO()
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }
                    bookingEVs = (await _unitOfWork.BookingEVRepository.GetAllBookingWithDetailAsync())
                                    .Where(b => b.DealerId == dealer.Id)
                                    .ToList();
                }

                var getBookingEVs = _mapper.Map<List<GetBookingEVDTO>>(bookingEVs);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all bookings successfully",
                    StatusCode = 200,
                    Result = getBookingEVs
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> GetBookingEVByIdAsync(ClaimsPrincipal user , Guid bookingId)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                var bookingEV = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingId);
                if(bookingEV == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found",
                        StatusCode = 404
                    };
                }

                if (role == StaticUserRole.Admin || role == StaticUserRole.EVMStaff)
                {
                    
                }
                else
                {
                    var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, CancellationToken.None);

                    if (dealer == null)
                    {
                        return new ResponseDTO()
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }
                }

                var getBookingEV = _mapper.Map<GetBookingEVDTO>(bookingEV);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Booking retrieved successfully",
                    StatusCode = 200,
                    Result = getBookingEV
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> UpdateBookingStatusAsync(ClaimsPrincipal user, Guid bookingId, BookingStatus newStatus)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 404
                    };
                }

                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User role not found.",
                        StatusCode = 403
                    };
                }

                var bookingEV = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingId);
                if (bookingEV == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found.",
                        StatusCode = 404
                    };
                }

                if (bookingEV.Status == BookingStatus.Cancelled ||
                    bookingEV.Status == BookingStatus.Approved ||
                    bookingEV.Status == BookingStatus.Rejected)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Cannot update status of a cancelled, approved, or rejected booking.",
                        StatusCode = 400
                    };
                }

                if (newStatus == BookingStatus.Pending)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Cannot update status to pending.",
                        StatusCode = 400
                    };
                }

                if (newStatus == BookingStatus.Approved || newStatus == BookingStatus.Rejected)
                {
                    if (role != StaticUserRole.Admin && role != StaticUserRole.EVMStaff)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Only Admin or EVM Staff can approve or reject a booking.",
                            StatusCode = 403
                        };
                    }
                }

                if (newStatus == BookingStatus.Cancelled)
                {
                    if (role != StaticUserRole.DealerManager)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Only Dealer Manager can cancel a booking.",
                            StatusCode = 403
                        };
                    }

                    if (bookingEV.Status != BookingStatus.Pending)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Can only cancel a pending booking.",
                            StatusCode = 400
                        };
                    }
                }

                if (newStatus == BookingStatus.Approved)
                {
                    var warehouse = await _unitOfWork.WarehouseRepository.GetWarehouseByDealerIdAsync(bookingEV.DealerId);
                    if (warehouse == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer's warehouse not found.",
                            StatusCode = 404
                        };
                    }

                    foreach (var dt in bookingEV.BookingEVDetails)
                    {
                        var pendingVehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetPendingVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                        if (pendingVehicles == null || !pendingVehicles.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "No available vehicles.",
                                StatusCode = 400
                            };
                        }

                        if (pendingVehicles.Count() < dt.Quantity)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Not enough vehicles available.",
                                StatusCode = 400
                            };
                        }

                        var selectedVehicles = pendingVehicles
                            .OrderBy(ev => ev.ImportDate)
                            .Take(dt.Quantity)
                            .ToList();

                        foreach (var ev in selectedVehicles)
                        {
                            ev.Status = ElectricVehicleStatus.Booked;
                            ev.WarehouseId = warehouse.Id;
                            _unitOfWork.ElectricVehicleRepository.Update(ev);
                        }
                    }
                }

                if (newStatus == BookingStatus.Rejected)
                {
                    foreach (var dt in bookingEV.BookingEVDetails)
                    {
                        var pendingVehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetPendingVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                        if (pendingVehicles == null || !pendingVehicles.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "No vehicles in pending status.",
                                StatusCode = 404
                            };
                        }

                        var selectedVehicles = pendingVehicles
                            .OrderBy(ev => ev.ImportDate)
                            .Take(dt.Quantity)
                            .ToList();

                        foreach (var ev in selectedVehicles)
                        {
                            ev.Status = ElectricVehicleStatus.Available;
                            _unitOfWork.ElectricVehicleRepository.Update(ev);
                        }
                    }
                }

                if (newStatus == BookingStatus.Cancelled)
                {
                    foreach (var dt in bookingEV.BookingEVDetails)
                    {
                        var pendingVehicles = await _unitOfWork.ElectricVehicleRepository
                            .GetPendingVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                        if (pendingVehicles == null || !pendingVehicles.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "No vehicles in pending status.",
                                StatusCode = 404
                            };
                        }

                        var selectedVehicles = pendingVehicles
                            .OrderBy(ev => ev.ImportDate)
                            .Take(dt.Quantity)
                            .ToList();

                        foreach (var ev in selectedVehicles)
                        {
                            ev.Status = StatusVehicle.Available;
                            _unitOfWork.ElectricVehicleRepository.Update(ev);
                        }
                    }
                }

                bookingEV.Status = newStatus;
                _unitOfWork.BookingEVRepository.Update(bookingEV);
                await _unitOfWork.SaveAsync();

                string message = newStatus switch
                {
                    BookingStatus.Approved => "Booking approved successfully.",
                    BookingStatus.Rejected => "Booking rejected successfully.",
                    BookingStatus.Cancelled => "Booking cancelled successfully.",
                    _ => "Booking status updated successfully."
                };

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = message,
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


        private async Task<ResponseDTO> UpdateQuantityRealTime(Guid versionId, Guid colorId, int quantity)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveElectricVehicleQuantityUpdate", versionId, colorId, quantity);


            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Real-time quantity update sent successfully",
                StatusCode = 200
            };
        }
    }
}
