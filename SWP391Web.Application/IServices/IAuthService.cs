using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.Entities;
using System.Security.Claims;
namespace SWP391Web.Application.IService
{
    public interface IAuthService
    {
        Task<ResponseDTO> LoginUser(LoginUserDTO loginUserDTO);
        Task<ResponseDTO> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO);
        Task<ResponseDTO> ResetPassword(ResetPasswordDTO resetPasswordDTO);
        Task<ResponseDTO> ChangePassword(ChangePasswordDTO changePasswordDTO, ClaimsPrincipal userClaims);
        Task<ResponseDTO> HandleGoogleCallbackAsync(ClaimsPrincipal userClaims);
    }
}
