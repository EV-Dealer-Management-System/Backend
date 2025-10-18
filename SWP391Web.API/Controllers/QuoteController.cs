using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Quote;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;
using SWP391Web.Domain.Enums;

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
            var response = await _quoteService.CreateQuoteAsync(User, createQuoteDTO);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("get-all-quote")]
        public async Task<ActionResult<ResponseDTO>> GetAllQuoteAsync()
        {
            var response = await _quoteService.GetAllAsync(User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("get-quote-by-id/{quoteId}")]
        public async Task<ActionResult<ResponseDTO>> GetQuoteByIdAsync([FromRoute] Guid quoteId)
        {
            var response = await _quoteService.GetQuoteByIdAsync(User, quoteId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("Update-quote-status/{quoteId}")]
        public async Task<ActionResult<ResponseDTO>> UpdateQuoteStatusAsync(Guid quoteId , [FromQuery] QuoteStatus newStatus)
        {
            var response = await _quoteService.UpdateQuoteStatusAsync(User, quoteId, newStatus);
            return StatusCode(response.StatusCode, response);
        }
    }
}
