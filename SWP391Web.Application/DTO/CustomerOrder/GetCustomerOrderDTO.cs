using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.CustomerOrder
{
    public record GetCustomerOrderDTO
    {
        public Guid CustomerOrderId { get; init; }
        public string CustomerName { get; init; } = default!;
        public string UserId { get; init; } = default!;
        public int OrderNo { get; init; }
    }
}
