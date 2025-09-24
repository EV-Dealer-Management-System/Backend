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


        // Orchestrator: create PDF + push + send
        [HttpPost("dealer-contracts")]
        public async Task<ActionResult<ResponseDTO>> CreateDealerContract([FromBody] CreateDealerContractDto dto, CancellationToken ct)
        {
            var r = await _svc.CreateAndSendAsync(dto, ct);
            return StatusCode(r.StatusCode, r);
        }


        // Create document only (multipart)
        [HttpPost("documents")] // form: file + fields
        public async Task<ActionResult<ResponseDTO>> CreateDocument([FromForm] CreateDocumentForm form, CancellationToken ct)
        {
            await using var stream = form.File.OpenReadStream();
            var req = new VnptCreateDocReq(form.No, form.Subject, form.TypeId, form.DepartmentId, form.Description);
            var r = await _svc.CreateDocumentAsync(req, stream, form.File.FileName, ct);
            return StatusCode(r.StatusCode, r);
        }
    }
}
