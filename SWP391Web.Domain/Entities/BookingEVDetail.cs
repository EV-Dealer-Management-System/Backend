using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class BookingElectricVehicleDetail
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public int Quantity { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        
    }
}
