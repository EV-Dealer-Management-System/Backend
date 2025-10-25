using SWP391Web.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IPaymentService
    {
        Task<ResponseDTO> CreateVNPayLink(Guid customerOrderId, CancellationToken ct);
    }
}
