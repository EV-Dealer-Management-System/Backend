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
    }
}
