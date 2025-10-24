using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Customer;
using System.Security.Claims;

namespace SWP391Web.Application.IServices
{
    public interface ICustomerService
    {
        Task<ResponseDTO> CreateCustomerAsync(ClaimsPrincipal user,CreateCustomerDTO createCustomerDTO);
        Task<ResponseDTO> GetAllCustomerAsync(ClaimsPrincipal user);
        Task<ResponseDTO> GetCustomerByIdAsync(ClaimsPrincipal user,Guid customerId);
        Task<ResponseDTO> UpdateCustomerAsync(ClaimsPrincipal user,Guid customerId, UpdateCustomerDTO updateCustomerDTO);
    }
}
