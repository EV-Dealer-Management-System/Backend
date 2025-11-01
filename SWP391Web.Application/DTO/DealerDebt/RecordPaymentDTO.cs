using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.DealerDebt
{
    public class RecordPaymentDTO
    {
        public decimal Amount { get; set; }
        public DateTime PaidAtUtc { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Note { get; set; }
        public string? Method { get; set; }
    }
}
