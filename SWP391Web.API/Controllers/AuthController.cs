using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IService;
using SWP391Web.Domain.Constants;
using System.Reflection;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "localhost",
            "localhost:5173",
            StaticLinkUrl.HostWebsite,
            StaticLinkUrl.HostApi
        };
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

        [AllowAnonymous]
        [HttpGet]
        [Route("google-callback")]
        public async Task<ActionResult<ResponseDTO>> GoogleCallBack([FromQuery] string? returnUrl)
        {
            var cookie = await HttpContext.AuthenticateAsync("External");
            if (!cookie.Succeeded || cookie.Principal is null)
            {
                var err = Uri.EscapeDataString("Google auth failed");
                return Redirect($"{SafeReturn(returnUrl)}?error={err}");
            }

            var res = await _authService.HandleGoogleCallbackAsync(cookie.Principal);
            await HttpContext.SignOutAsync("External");

            if (!res.IsSuccess)
            {
                var err = Uri.EscapeDataString(res.Message ?? "Google login failed");
                return Redirect($"{SafeReturn(returnUrl)}?error={err}");
            }

            var vm = JsonSerializer.Deserialize<AuthResultDTO>(JsonSerializer.Serialize(res.Result), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            if (vm is null || string.IsNullOrWhiteSpace(vm.AccessToken))
                return StatusCode(500, new { message = "No AccessToken in Result" });

            var ticket = Guid.NewGuid().ToString("N");
            await _authService.StoreAsync(ticket, vm, TimeSpan.FromMinutes(2));

            var sep = SafeReturn(returnUrl).Contains('?') ? "&" : "?";
            return Redirect($"{SafeReturn(returnUrl)}{sep}ticket={ticket}");
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("signin-google")]
        public IActionResult SignInGoogle([FromQuery] string? returnUrl)
        {
            var defaultReturn = $"{StaticLinkUrl.WebUrl}/login-success";
            var safeReturn = returnUrl ?? defaultReturn;

            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallBack), "Auth", new { returnUrl = safeReturn }, Request.Scheme)
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet("exchange")]
        public async Task<IActionResult> Exchange([FromQuery] string ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket)) return BadRequest(new { message = "Missing ticket" });

            var payload = await _authService.RedeemAsync(ticket);
            if (payload is null) return Unauthorized(new { message = "Ticket invalid or expired" });

            return Ok(payload);
        }

        private string SafeReturn(string? returnUrl)
        {
            var @default = $"{StaticLinkUrl.WebUrl}/login-success";
            if (string.IsNullOrWhiteSpace(returnUrl)) return @default;
            return returnUrl;
        }
    }
}
