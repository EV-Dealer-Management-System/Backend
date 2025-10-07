using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class BookingEV
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; } // Đại lý thực hiện booking
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; } // Enum trạng thái
        public int TotalQuantity { get; set; }
        public string? Note { get; set; }
        public string? CreatedBy { get; set; }

        public ICollection<BookingEVDetail> BookingEVDetails { get; set; } = new List<BookingEVDetail>();
        public Dealer Dealer { get; set; } = null!;
    }
}
