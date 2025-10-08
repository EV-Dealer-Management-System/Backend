using SWP391Web.Application.DTO.BookingEVDetail;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.BookingEV
{
    public class CreateBookingEVDTO
    {
        public Guid DealerId { get; set; } // Đại lý thực hiện booking
        public string? Note { get; set; }
        public List<CreateBookingEVDetailDTO> BookingDetails { get; set; } = new();
    }
}
