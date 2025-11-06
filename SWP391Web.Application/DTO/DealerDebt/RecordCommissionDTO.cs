using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.DealerDebt
{
    public class RecordCommissionDTO
    {
        public decimal Amount { get; set; }
        public DateTime AtUtc { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Note { get; set; }
    }
}
