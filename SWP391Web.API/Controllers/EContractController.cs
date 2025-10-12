using EVManagementSystem.Application.DTO.EContract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EContract;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Pdf;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Enums;
using SWP391Web.Domain.ValueObjects;
using System.Text;

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


        [HttpGet]
        [Route("get-info-to-sign-process-by-code")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> GetInfoSignProcess([FromQuery] string processCode)
        {
            var r = await _svc.GetAccessTokenAsyncByCode(processCode);
            return Ok(r);
        }

        [HttpGet]
        [Route("get-access-token-for-evc")]
        [Authorize(Roles = StaticUserRole.Admin)]
        public async Task<ActionResult<ResponseDTO>> GetAccessToken()
        {
            var r = await _svc.GetAccessTokenAsync();
            return Ok(r);
        }
        
        [HttpPost]
        [Route("ready-dealer-contracts")]
        [Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> CreateEContractAsync([FromBody] CreateEContractDTO dto, CancellationToken ct)
        {
            var r = await _svc.CreateEContractAsync(User, dto, ct);
            return StatusCode(r.StatusCode, r);
        }

        [HttpPost]
        [Route("draft-dealer-contracts")]
        [Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> CreateDraftDealerContract([FromBody] CreateDealerDTO dto, CancellationToken ct)
        {
            var r = await _svc.CreateDraftEContractAsync(User, dto, ct);
            return StatusCode(r.StatusCode, r);
        }

        // Sign process
        [HttpPost]
        [Route("sign-process")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> SignProcess([FromQuery] string token, [FromBody] VnptProcessDTO dto, CancellationToken ct)
        {
            var r = await _svc.SignProcess(token, dto, ct);
            return StatusCode(r.StatusCode, r);
        }


        [HttpGet]
        [Route("preview")]
        [AllowAnonymous]
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

        [HttpGet]
        [Route("preview-html")]
        [AllowAnonymous]
        public async Task<IActionResult> PreviewHtml([FromQuery] string downloadUrl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
                return BadRequest("Missing downloadUrl");

            // 1. Download PDF từ VNPT
            var upstream = await _svc.GetHtmtEContractAsync(downloadUrl, ct);
            if (!upstream.IsSuccessStatusCode)
            {
                var err = await upstream.Content.ReadAsStringAsync(ct);
                return StatusCode((int)upstream.StatusCode, err);
            }

            // 2. Lưu tệp PDF tạm
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var pdfPath = Path.Combine(tempDir, "input.pdf");
            await using (var fs = System.IO.File.Create(pdfPath))
                await upstream.Content.CopyToAsync(fs, ct);

            // 3. Convert PDF → HTML qua Docker
            var htmlPath = await EContractPdf.ConvertPdfToHtmlAsync(pdfPath);

            // 4. Trả về HTML
            var html = await System.IO.File.ReadAllTextAsync(htmlPath, Encoding.UTF8);
            return Content(html, "text/html", Encoding.UTF8);
        }

        [HttpPost]
        [Route("add-smartca")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> AddSmartCA([FromBody] AddNewSmartCADTO dto)
        {
            var r = await _svc.AddSmartCA(dto);
            return Ok(r);
        }

        [HttpGet]
        [Route("smartca-info/{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> GetSmartCAInformation([FromRoute] int userId)
        {
            var r = await _svc.GetSmartCAInformation(userId);
            return Ok(r);
        }

        [HttpPost]
        [Route("update-smartca")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO>> UpdateSmartCA([FromBody] UpdateSmartDTO dto)
        {
            var r = await _svc.UpdateSmartCA(dto);
            return Ok(r);
        }

        [HttpPost]
        [Route("update-econtract")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> UpdateEContract([FromBody] UpdateEContractDTO dto)
        {
            var r = await _svc.UpdateEContract(dto);
            return Ok(r);
        }

        [HttpGet]
        [Route("get-econtract-list")]
        //[Authorize(Roles = StaticUserRole.Admin_EVMStaff)]
        public async Task<ActionResult<ResponseDTO>> GetEContractList([FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] EContractStatus eContractStatus)
        {
            var r = await _svc.GetEContractList(pageNumber, pageSize, eContractStatus);
            return Ok(r);
        }

    }
}
