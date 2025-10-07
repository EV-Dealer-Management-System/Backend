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

        public async Task<ResponseDTO> CreateBookingEVAsync(CreateBookingEVDTO createBookingEVDTO)
        {
            try
            {
                //var existDealer = await _unitOfWork.DealerRepository.IsExistByIdAsync(createBookingEVDTO.DealerId);

                var bookingEV = new BookingEV
                {
                    DealerId = createBookingEVDTO.DealerId,
                    Note = createBookingEVDTO.Note,
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Pending,
                    CreatedBy = "Dealer",
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

                var getBookingEV =  _mapper.Map<GetBookingEVDTO>(bookingEV);

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
    }
}
