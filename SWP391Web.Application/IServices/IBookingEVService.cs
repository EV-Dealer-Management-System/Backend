using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IBookingEVService
    {
        Task <ResponseDTO> CreateBookingEVAsync(ClaimsPrincipal user, CreateBookingEVDTO createBookingEVDTO);
        Task <ResponseDTO> GetAllBookingEVsAsync(ClaimsPrincipal user);
        Task <ResponseDTO> GetBookingEVByIdAsync(ClaimsPrincipal user, Guid bookingId);
        Task <ResponseDTO> UpdateBookingStatusAsync(Guid bookingId, BookingStatus newStatus);

    }
}
