using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using SWP391Web.Application.DTO.BookingEVDetail;
using SWP391Web.Application.IServices;

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
            var response = await _bookingEVService.CreateBookingEVAsync(createBookingEVDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-all-bookings")]
        public async Task<ActionResult<ResponseDTO>> GetAllBookingEVs()
        {
            var response = await _bookingEVService.GetAllBookingEVsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-booking-by-id/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> GetBookingEVById([FromRoute] Guid bookingId)
        {
            var response = await _bookingEVService.GetBookingEVByIdAsync(bookingId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("cancel-booking/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> CancelBookingEV([FromRoute] Guid bookingId)
        {
            var response = await _bookingEVService.CancelBookingEVAsync(bookingId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("approve-booking/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> ApprovedBookingEVStatus([FromRoute] Guid bookingId)
        {
            var response = await _bookingEVService.ApprovedBookingEVStatusAsync(bookingId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("reject-booking/{bookingId}")]
        public async Task<ActionResult<ResponseDTO>> RejectedBookingEVStatus([FromRoute] Guid bookingId)
        {
            var response = await _bookingEVService.RejectedBookingEVStatusAsync(bookingId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
