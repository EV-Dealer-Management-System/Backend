using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class VehicleColor
    {
        [Key]  // Bắt buộc
        public Guid ColorId { get; set; }
        [ForeignKey("Vehicle")]
        public Guid VehicleId { get; set; }
        public string? ColorName { get; set; }
        public decimal ExtraPrice { get; set; }
    }
}
