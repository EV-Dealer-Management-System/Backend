using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.CustomerOrder;
using System.Security.Claims;

namespace SWP391Web.Application.IServices
{
    public interface ICustomerOrderService
    {
        Task<ResponseDTO> CreateCustomerOrderAsync(ClaimsPrincipal user, CreateOrderDTO createOrderDTO, CancellationToken ct);
    }
}
