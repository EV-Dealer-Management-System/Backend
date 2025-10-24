using SWP391Web.Application.DTO.OrderDetail;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.CustomerOrder
{
    public class CreateCustomerOrderDTO
    {
        public Guid CustomerId { get; set; }
        public Guid QuoteId { get; set; }
        public int OrderNo { get; set; }
        public OrderStatus Status { get; set; }
    }
}
