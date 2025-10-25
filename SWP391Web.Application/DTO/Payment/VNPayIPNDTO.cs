using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.Payment
{
    public class VNPayIPNDTO
    {
        public string Vnp_TmnCode { get; set; } = null!;
        public string Vnp_Amount { get; set; } = null!;
        public string Vnp_BankCode { get; set; } = null!;
        public string? Vnp_BankTranNo { get; set; }
        public string? Vnp_CardType { get; set; }
        public string Vnp_PayDate { get; set; } = null!;
        public string Vnp_OrderInfo { get; set; } = null!;
        public string Vnp_TransactionNo { get; set; } = null!;
        public string Vnp_ResponseCode { get; set; } = null!;
        public string Vnp_TransactionStatus { get; set; } = null!;
        public string Vnp_TxnRef { get; set; } = null!;
        public string Vnp_SecureHash { get; set; } = null!;
    }
}
