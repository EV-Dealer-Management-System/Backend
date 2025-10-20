using SWP391Web.Application.DTO.EVTemplate;
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
        public ViewVersionName? Version { get; set; }
        public ViewColorName? Color { get; set; }
        public ViewPromotionName? Promotion { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class ViewPromotionName
    {
        public Guid PromotionId { get; set; }
        public string? PromotionName { get; set; }
    }
}
