using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IBookingEVService
    {
        Task <ResponseDTO> CreateBookingEVAsync(CreateBookingEVDTO createBookingEVDTO);
        Task <ResponseDTO> GetAllBookingEVsAsync();
        Task <ResponseDTO> GetBookingEVByIdAsync(Guid bookingId);
        Task <ResponseDTO> CancelBookingEVAsync(Guid bookingId);
        Task<ResponseDTO> ApprovedBookingEVStatusAsync(Guid bookingId);
        Task<ResponseDTO> RejectedBookingEVStatusAsync(Guid bookingId);


    }
}
