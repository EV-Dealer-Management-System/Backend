using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EContractController : ControllerBase
    {
        private readonly IEContractService _svc;
        public EContractController(IEContractService svc) 
        { 
            _svc = svc;
        }


        [HttpGet("get-info-to-sign-process-by-code")]
        public async Task<ActionResult<ResponseDTO>> GetInfoSignProcess([FromQuery] string processCode)
        {
            var r = await _svc.GetAccessTokenAsyncByCode(processCode);
            return Ok(r);
        }

        [HttpGet("get-access-token-for-evc")]
        public async Task<ActionResult<ResponseDTO>> GetAccessToken()
        {
            var r = await _svc.GetAccessTokenAsync();
            return Ok(r);
        }
        // Orchestrator: create PDF + push + send
        [HttpPost("dealer-contracts")]
        public async Task<ActionResult<ResponseDTO>> CreateDealerContract([FromBody] CreateDealerDTO dto)
        {
            var r = await _svc.CreateAndSendAsync(dto);
            return StatusCode(r.StatusCode, r);
        }

        // Sign process
        [HttpPost("sign-process")]
        public async Task<ActionResult<ResponseDTO>> SignProcess([FromQuery] string token, [FromBody] VnptProcessDTO dto)
        {
            var r = await _svc.SignProcess(token, dto);
            return StatusCode(r.StatusCode, r);
        }


        [HttpGet]
        [Route("preview")]
        public async Task<IActionResult> Preview([FromQuery] string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Missing token");

            Request.Headers.TryGetValue("Range", out var rangeHeader);

            var upstream = await _svc.GetPreviewResponseAsync(token, rangeHeader.ToString(), ct);
            HttpContext.Response.RegisterForDispose(upstream);

            if (!upstream.IsSuccessStatusCode)
            {
                var err = await upstream.Content.ReadAsStringAsync(ct);
                return StatusCode((int)upstream.StatusCode, err);
            }

            var stream = await upstream.Content.ReadAsStreamAsync(ct);
            var contentType = upstream.Content.Headers.ContentType?.ToString() ?? "application/pdf";

            Response.StatusCode = (int)upstream.StatusCode;

            if (upstream.Headers.AcceptRanges is { Count: > 0 })
                Response.Headers["Accept-Ranges"] = string.Join(",", upstream.Headers.AcceptRanges);

            if (upstream.Content.Headers.ContentRange is not null)
                Response.Headers["Content-Range"] = upstream.Content.Headers.ContentRange.ToString();

            if (upstream.Content.Headers.ContentLength is long len)
                Response.ContentLength = len;

            var fileName = upstream.Content.Headers.ContentDisposition?.FileNameStar
                           ?? upstream.Content.Headers.ContentDisposition?.FileName
                           ?? "document.pdf";
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            Response.ContentType = contentType;
            Response.Headers["Cache-Control"] = "no-store";
            return File(stream, contentType);
        }

        [HttpPost]
        [Route("add-smartca")]
        public async Task<ActionResult<ResponseDTO>> AddSmartCA([FromBody] AddNewSmartCADTO dto)
        {
            var r = await _svc.AddSmartCA(dto);
            return Ok(r); 
        }

        [HttpGet]
        [Route("smartca-info/{userId}")]
        public async Task<ActionResult<ResponseDTO>> GetSmartCAInformation([FromRoute] int userId)
        {
            var r = await _svc.GetSmartCAInformation(userId);
            return Ok(r);
        }
    }
}
