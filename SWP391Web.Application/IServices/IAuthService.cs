using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.Entities;
using System.Security.Claims;
namespace SWP391Web.Application.IService
{
    public interface IAuthService
    {
        Task<ResponseDTO> RegisterCustomer(RegisterCustomerDTO registerCustomerDTO);
        Task<ResponseDTO> ResendVerifyEmail(string email);
        Task<ResponseDTO> VerifyEmil(string userId, string token);
        Task<ResponseDTO> LoginUser(LoginUserDTO loginUserDTO);
        Task<ResponseDTO> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO);
        Task<ResponseDTO> ResetPassword(ResetPasswordDTO resetPasswordDTO);
    }
}
