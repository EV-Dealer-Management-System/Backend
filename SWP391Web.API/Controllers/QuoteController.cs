using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Quote;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuoteController : ControllerBase
    {
        public readonly IQuoteService _quoteService;
        public QuoteController(IQuoteService quoteService)
        {
            _quoteService = quoteService ?? throw new ArgumentNullException(nameof(quoteService));
        }
        [HttpPost("create-quote")]
        public async Task<ActionResult<ResponseDTO>> CreateQuoteAsync([FromBody] CreateQuoteDTO createQuoteDTO)
        {
            var response = await _quoteService.CreateQuoteAsync(User , createQuoteDTO);
            return StatusCode(response.StatusCode , response);
        }
    }
}
