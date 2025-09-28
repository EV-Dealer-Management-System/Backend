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
        public EContractController(IEContractService svc) { _svc = svc; }


        [HttpGet("get-access-toke")]
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
        public async Task<ActionResult<ResponseDTO>> SignProcess([FromBody] VnptProcessDTO dto)
        {
            var r = await _svc.SignProcess(dto);
            return StatusCode(r.StatusCode, r);
        }


        // Create document only (multipart)
        //[HttpPost("documents")] // form: file + fields
        //public async Task<ActionResult<ResponseDTO>> CreateDocument([FromForm] CreateDocumentForm form, CancellationToken ct)
        //{
        //    await using var stream = form.File.OpenReadStream();
        //    var req = new VnptCreateDocReq(form.No, form.Subject, form.TypeId, form.DepartmentId, form.Description);
        //    var r = await _svc.CreateDocumentAsync(req, stream, form.File.FileName, ct);
        //    return StatusCode(r.StatusCode, r);
        //}

        //[HttpGet("token")]
        //public async Task<ActionResult<string>> GetAccessToken(CancellationToken ct)
        //{
        //    var token = await _svc.GetAccessTokenAsync(ct);
        //    return Ok(token);
        //}

        //[HttpPost("create-new-document-v2")]
        //public async Task<IActionResult> CreateDocumentV2([FromQuery] Guid dealerId, CancellationToken ct)
        //{
        //    if (dealerId == Guid.Empty)
        //        return BadRequest("dealerId is required");

        //    var res = await _svc.CreateDocument(dealerId, ct);

        //    // Nếu VNPT trả lỗi thì đẩy status code + payload ra để debug nhanh
        //    if (!res.Success)
        //        return StatusCode(res.Code == 0 ? 500 : 200, res);

        //    return Ok(res);
        //}
    }
}
