using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using SWP391Web.Application.DTO.BookingEVDetail;
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
using UglyToad.PdfPig.Outline;

namespace SWP391Web.Application.Services
{
    public class BookingEVService : IBookingEVService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;

        public BookingEVService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseDTO> CancelBookingEVAsync(Guid bookingId)
        {
            try
            {
                var bookingEV = await _unitOfWork.BookingEVRepository
                    .GetBookingWithIdAsync(bookingId);
                if (bookingEV == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found",
                        StatusCode = 404,
                    };
                }

                bookingEV.Status = BookingStatus.Cancelled;

                _unitOfWork.BookingEVRepository.Update(bookingEV);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Booking cancelled successfully",
                    StatusCode = 200,
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

                await _unitOfWork.BookingEVRepository.AddAsync(bookingEV, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var bookingWithDetails = await _unitOfWork.BookingEVRepository
                    .GetBookingWithIdAsync(bookingEV.Id);

                var getBookingEV = _mapper.Map<GetBookingEVDTO>(bookingWithDetails);

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

        public Task<ResponseDTO> GetAllBookingEVsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDTO> GetBookingEVByIdAsync(Guid bookingId)
        {
            try
            {
                var bookingEV = await _unitOfWork.BookingEVRepository
                    .GetBookingWithIdAsync(bookingId);
                if (bookingEV == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found",
                        StatusCode = 404,
                    };
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

        public async Task<ResponseDTO> UpdateBookingStatusAsync(Guid bookingId, BookingStatus newStatus)
        {
            try
            {
                var bookingEV = await _unitOfWork.BookingEVRepository
                    .GetBookingWithIdAsync(bookingId);
                if (bookingEV == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found",
                        StatusCode = 404,
                    };
                }

                //khong cho phep update neu da bi huy, duyet hoac tu choi
                if (bookingEV.Status == BookingStatus.Cancelled
                    || bookingEV.Status == BookingStatus.Approved
                    || bookingEV.Status == BookingStatus.Rejected)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Cannot update status of a cancelled, approved or rejected booking",
                        StatusCode = 400,
                    };
                }

                //ktra logic hop li de chuyen status
                if (newStatus == BookingStatus.Pending)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Cannot update status to pending",
                        StatusCode = 400,
                    };
                }

                bookingEV.Status = newStatus;
                _unitOfWork.BookingEVRepository.Update(bookingEV);
                await _unitOfWork.SaveAsync();

                string message = newStatus switch
                {
                    BookingStatus.Approved => "Booking approved successfully",
                    BookingStatus.Rejected => "Booking rejected successfully",
                    BookingStatus.Cancelled => "Booking cancelled successfully",
                    _ => "Booking status updated successfully"
                };

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = message,
                    StatusCode = 200,
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
    }
}
