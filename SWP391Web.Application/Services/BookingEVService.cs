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
using System.Text;
using System.Threading.Tasks;

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

        public async Task<ResponseDTO> CreateBookingEVAsync(CreateBookingEVDTO createBookingEVDTO)
        {
            try
            {
                var isExistDealer = await _unitOfWork.DealerRepository.IsExistByIdAsync(createBookingEVDTO.DealerId,CancellationToken.None);
                if (!isExistDealer)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404,
                    };
                }
                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(createBookingEVDTO.DealerId, CancellationToken.None);
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
                    DealerId = createBookingEVDTO.DealerId,
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

                await _unitOfWork.BookingEVRepository.AddAsync(bookingEV,CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var bookingWithDetails = await _unitOfWork.BookingEVRepository
                    .GetBookingWithIdAsync(bookingEV.Id);

                var getBookingEV =  _mapper.Map<GetBookingEVDTO>(bookingWithDetails);

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

        public　async Task<ResponseDTO> GetBookingEVByIdAsync(Guid bookingId)
        {
            try
            {
                var bookingEV =  await _unitOfWork.BookingEVRepository
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

        public async Task<ResponseDTO> ApprovedBookingEVStatusAsync(Guid bookingId)
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
                        Message = " Booking not found ",
                        StatusCode = 404,
                    };
                }

                if(bookingEV.Status != BookingStatus.Pending)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Invalid status value ",
                        StatusCode = 400,
                    };
                }

                bookingEV.Status = BookingStatus.Approved;
                

                _unitOfWork.BookingEVRepository.Update(bookingEV);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = " Booking approved successfully ",
                    StatusCode = 200,
                };


            }
            catch (Exception ex) {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> RejectedBookingEVStatusAsync(Guid bookingId)
        {
            try
            {
                var bookingEV = await _unitOfWork.BookingEVRepository
                    .GetBookingWithIdAsync(bookingId);
                if (bookingEV is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Booking not found",
                        StatusCode = 404,
                    };
                }

                if (bookingEV.Status != BookingStatus.Pending)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid status value",
                        StatusCode = 404,
                    };
                }

                bookingEV.Status = BookingStatus.Rejected;
                _unitOfWork.BookingEVRepository.Update(bookingEV);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = " Reject booking successfully ",
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
