using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using SWP391Web.Application.DTO.BookingEVDetail;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Enums;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingEVController : ControllerBase
    {
        public readonly IBookingEVService _bookingEVService;
        public BookingEVController(IBookingEVService bookingEVService)
        {
            _bookingEVService = bookingEVService ?? throw new ArgumentNullException(nameof(bookingEVService));
        }
        [HttpPost("create-booking")]
        public  async Task<ActionResult<ResponseDTO>> CreateBookingEV([FromBody] CreateBookingEVDTO createBookingEVDTO)
        {
            var response = await _bookingEVService.CreateBookingEVAsync(User, createBookingEVDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-all-bookings")]
        public async Task<ActionResult<ResponseDTO>> GetAllBookingEVs()
        {
            var response = await _bookingEVService.GetAllBookingEVsAsync(User);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-booking-by-id/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> GetBookingEVById([FromRoute] Guid bookingId)
        {
            var response = await _bookingEVService.GetBookingEVByIdAsync(User,bookingId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update-booking-status/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateBookingStatus(Guid bookingId, [FromQuery] BookingStatus newStatus)
        {
            var response = await _bookingEVService.UpdateBookingStatusAsync(User, bookingId, newStatus);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-vehicles-by-booking-id/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> GetVehicleByBookingId([FromRoute] Guid bookingId)
        {
            var response = await _bookingEVService.GetVehicleByBookingIdAsync(bookingId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
