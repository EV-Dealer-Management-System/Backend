using System.ComponentModel.DataAnnotations;

namespace SWP391Web.Domain.Entities
{
    public class CustomerOrder
    {
        public Guid CustomerOrderId { get; set; }
        public string CustomerName { get; set; } = default!;
        public int OrderNo { get; set; }
    }
}
