using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IService;
using SWP391Web.Domain.Constants;

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

        [HttpPost]
        [Route("change-password")]
        public async Task<ActionResult<ResponseDTO>> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            var response = await _authService.ChangePassword(changePasswordDTO, User);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("google-callback")]
        public async Task<ActionResult<ResponseDTO>> GoogleCallBack([FromQuery] string? returnUrl)
        {
            var cookie = await HttpContext.AuthenticateAsync("External");
            if (!cookie.Succeeded || cookie.Principal is null)
            {
                return Unauthorized();
            }

            var response = await _authService.HandleGoogleCallbackAsync(cookie.Principal);
            if (!response.IsSuccess)
                return StatusCode(response.StatusCode, response);

            await HttpContext.SignOutAsync("External");

            var token = ((dynamic)response.Result).AccessToken;
            var url = (returnUrl ?? $"{StaticLinkUrl.WebUrl}/login-success#token={token}");
            return Redirect(url);
        }

        [HttpGet]
        [Route("signin-google")]
        public IActionResult SignInGoogle([FromQuery] string? returnUrl)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallBack), "Auth", new { returnUrl }, Request.Scheme)
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
    }
}
