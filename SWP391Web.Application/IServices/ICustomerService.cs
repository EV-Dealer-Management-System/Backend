using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Customer;
using System.Security.Claims;

namespace SWP391Web.Application.IServices
{
    public interface ICustomerService
    {
        Task<ResponseDTO> CreateCustomerAsync(ClaimsPrincipal user,CreateCustomerDTO createCustomerDTO);
        Task<ResponseDTO> GetAllCustomer(ClaimsPrincipal user);
        Task<ResponseDTO> GetCustomerById(ClaimsPrincipal user,Guid customerId);
    }
}
