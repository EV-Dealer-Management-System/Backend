using SWP391Web.Application.DTO.QuoteDetail;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.Quote
{
    public class CreateQuoteDTO
    {
        public Guid PromotionId { get; set; }

        public string? Note { get; set; }
        public List<GetQuoteDetailDTO> QuoteDetails { get; set; } = new();
    }
}
