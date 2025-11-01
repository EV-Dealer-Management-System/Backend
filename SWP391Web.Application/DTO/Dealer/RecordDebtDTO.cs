using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.Dealer
{
    public class RecordDebtDTO
    {
        public string? ReferenceNo { get; set; }
        public DateTime ConfirmDateUtc { get; set; }
        public decimal Amount { get; set; }
    }
}
