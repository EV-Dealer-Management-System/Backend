using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Payment;
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
        Task<ResponseDTO> HandleVNPayIpn(VNPayIPNDTO ipnDTO, CancellationToken ct);
        Task<ResponseDTO> CreateVNPayLinkMobile(int amount, CancellationToken ct);
        Task<ResponseDTO> GetAllMobile(int pageNumber, int pageSize, CancellationToken ct);
    }
}
