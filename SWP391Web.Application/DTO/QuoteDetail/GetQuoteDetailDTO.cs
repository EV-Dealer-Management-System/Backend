using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.QuoteDetail
{
    public class GetQuoteDetailDTO
    {
        public Guid Id { get; set; }
        public Guid QuoteId { get; set; }
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public Guid PromotionId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
