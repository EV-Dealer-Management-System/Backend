using EVManagementSystem.Application.DTO.EContract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EContract;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Enums;
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
        [HttpPost("ready-dealer-contracts")]
        public async Task<ActionResult<ResponseDTO>> CreateEContractAsync([FromBody] CreateEContractDTO dto, CancellationToken ct)
        {
            var r = await _svc.CreateEContractAsync(User, dto, ct);
            return StatusCode(r.StatusCode, r);
        }

        [HttpPost]
        [Route("draft-dealer-contracts")]
        public async Task<ActionResult<ResponseDTO>> CreateDraftDealerContract([FromBody] CreateDealerDTO dto, CancellationToken ct)
        {
            var r = await _svc.CreateDraftEContractAsync(User, dto, ct);
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
        public async Task<IActionResult> Preview([FromQuery] string downloadURL, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(downloadURL))
                return BadRequest("Missing downloadURL");

            Request.Headers.TryGetValue("Range", out var rangeHeader);

            var upstream = await _svc.GetPreviewResponseAsync(downloadURL, rangeHeader.ToString(), ct);
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

            var upstreamCd = upstream.Content.Headers.ContentDisposition;
            var safeFileName = upstreamCd?.FileNameStar ?? upstreamCd?.FileName ?? "document.pdf";

            var cd = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = safeFileName
            };
            Response.Headers[HeaderNames.ContentDisposition] = cd.ToString();

            Response.ContentType = contentType;
            Response.Headers[HeaderNames.CacheControl] = "no-store";

            return new FileStreamResult(stream, contentType);
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

        [HttpPost]
        [Route("update-smartca")]
        public async Task<ActionResult<ResponseDTO>> UpdateSmartCA([FromBody] UpdateSmartDTO dto)
        {
            var r = await _svc.UpdateSmartCA(dto);
            return Ok(r);
        }

        [HttpPost]
        [Route("update-econtract")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseDTO>> UpdateEContract([FromForm] UpdateEContractDTO dto)
        {
            if (dto.File is null || dto.File.Length == 0)
                return BadRequest(new { message = "file is missing." });

            if (dto.File.ContentType?.Contains("pdf", StringComparison.OrdinalIgnoreCase) != true)
                return BadRequest(new { message = "File is not PDF." });

            var r = await _svc.UpdateEContract(dto);
            return Ok(r);
        }

        [HttpGet]
        [Route("get-econtract-list")]
        public async Task<ActionResult<ResponseDTO>> GetEContractList([FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] EContractStatus eContractStatus)
        {
            var r = await _svc.GetEContractList(pageNumber, pageSize, eContractStatus);
            return Ok(r);
        }

    }
}
