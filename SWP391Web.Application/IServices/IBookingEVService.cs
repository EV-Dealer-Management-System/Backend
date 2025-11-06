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
        Task<ResponseDTO> CreateBookingEVAsync(ClaimsPrincipal user, CreateBookingEVDTO createBookingEVDTO, CancellationToken ct);
        Task<ResponseDTO> GetAllBookingEVsAsync(ClaimsPrincipal user, int pageNumber, int pageSize, BookingStatus? bookingStatus, CancellationToken ct);
        Task <ResponseDTO> GetBookingEVByIdAsync(Guid bookingId);
        Task <ResponseDTO> UpdateBookingStatusAsync(ClaimsPrincipal user, Guid bookingId, BookingStatus newStatus);
        Task<ResponseDTO> GetVehicleByBookingIdAsync(Guid bookingId);
        Task<ResponseDTO> ConfirmBookingDeliveryAsync(ClaimsPrincipal user, Guid bookingId, CancellationToken ct);
    }
}
