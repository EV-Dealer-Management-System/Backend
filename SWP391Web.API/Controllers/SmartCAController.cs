using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmartCAController : ControllerBase
    {
        private readonly ISmartCAService _smartCAService;
        public SmartCAController(ISmartCAService smartCAService)
        {
            _smartCAService = smartCAService ?? throw new ArgumentNullException(nameof(smartCAService));
        }

        [HttpPost]
        [Route("test-get-access-token-smartCA")]
        public async Task<ActionResult<ResponseDTO>> TestGetAccessTokenSmartCA()
        {
            var response = await _smartCAService.GetAccessTokenAsync();
            return Ok(response);
        }
    }
}
