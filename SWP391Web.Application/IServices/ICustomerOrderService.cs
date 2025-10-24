using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.CustomerOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface ICustomerOrderService
    {
        Task<ResponseDTO> CreateCustomerOrderAsync( ClaimsPrincipal user,CreateCustomerOrderDTO createCustomerOrderDTO);
    }
}
