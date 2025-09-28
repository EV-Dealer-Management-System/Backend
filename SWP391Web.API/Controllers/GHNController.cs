using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Application.Services;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GHNController : ControllerBase
    {
        private readonly IGHNService _ghn;
        public GHNController(IGHNService ghn)
        {
            _ghn = ghn;
        }

        [HttpGet]
        [Route("get-provices")]
        public async Task<ActionResult<ResponseDTO>> GetProvincesAsync()
        {
            var response = await _ghn.GetProvincesAsync();
            return StatusCode(response.StatusCode, response);
        }

    }
}
