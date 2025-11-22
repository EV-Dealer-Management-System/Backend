using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        public readonly ILogService _logService;
        public LogController(ILogService logService)
        {
            _logService = logService;
        }
        [HttpGet("get-all-logs")]
        public async Task<ActionResult<ResponseDTO>> GetAllLogs(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var response = await _logService.GetAllLogsAsync(User,pageNumber, pageSize, ct);
            return StatusCode(response.StatusCode, response);

        }
    }
}
