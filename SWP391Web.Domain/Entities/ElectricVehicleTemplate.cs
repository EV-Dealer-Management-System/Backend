using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ElectricVehicleTemplate
    {
        public Guid Id { get; set; }                  
        public Guid VersionId { get; set; }          
        public Guid ColorId { get; set; }             
        public decimal Price { get; set; }            
        public string Description { get; set; }
    }
}
