using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ElectricVehicleColor
    {
        public Guid Id { get; set; }
        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }
        public decimal ExtraCost { get; set; }

        public ICollection<ElectricVehicle> ElectricVehicles { get; set; } = new List<ElectricVehicle>();
        public ICollection<BookingEVDetail> BookingEVDetails { get; set; } = new List<BookingEVDetail>();
        public ICollection<QuoteDetail> QuoteDetails { get; set; } = new List<QuoteDetail>();
    }
}
