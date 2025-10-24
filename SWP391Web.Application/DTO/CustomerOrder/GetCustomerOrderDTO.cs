using SWP391Web.Application.DTO.OrderDetail;
using SWP391Web.Application.DTO.QuoteDetail;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.CustomerOrder
{
    public record GetCustomerOrderDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid QuoteId { get; set; }
        public int OrderNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<GetQuoteDetailDTO> QuoteDetails { get; set; } = new();
    }
}
