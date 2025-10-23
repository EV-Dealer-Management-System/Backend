using SWP391Web.Application.DTO.Auth;
using System.Security.Claims;

namespace SWP391Web.Application.IServices
{
    public interface ICustomerService
    {
        Task<ResponseDTO> GetUserProfile(ClaimsPrincipal user);
    }
}
