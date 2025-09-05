using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IService;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("register-customer")]
        public async Task<ActionResult<ResponseDTO>> RegisterCustomer([FromBody] RegisterCustomerDTO registerCustomerDTO)
        {
            var response = await _authService.RegisterCustomer(registerCustomerDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("resend-verify-email")]
        public async Task<ActionResult<ResponseDTO>> SendVerifyEmail([FromBody] ResendVerifyEmailDTO resendVerifyEmailDTO)
        {
            var response = await _authService.ResendVerifyEmail(resendVerifyEmailDTO.Email);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<ResponseDTO>> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var response = await _authService.VerifyEmil(userId, token);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("login-user")]
        public async Task<ActionResult<ResponseDTO>> LoginUser([FromBody] LoginUserDTO loginUserDTO)
        {
            var response = await _authService.LoginUser(loginUserDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ResponseDTO>> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
        {
            var response = await _authService.ForgotPassword(forgotPasswordDTO);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ResponseDTO>> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var response = await _authService.ResetPassword(resetPasswordDTO);
            return StatusCode(response.StatusCode, response);
        }
    }
}
