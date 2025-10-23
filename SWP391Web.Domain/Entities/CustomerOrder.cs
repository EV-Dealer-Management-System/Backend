using SWP391Web.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SWP391Web.Domain.Entities
{
    public class CustomerOrder
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid QuoteId { get; set; }
        public int OrderNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public Customer Customer { get; set; } = null!;
        public Quote Quote { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
