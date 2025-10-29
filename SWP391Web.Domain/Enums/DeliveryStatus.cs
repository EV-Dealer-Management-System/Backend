using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Enums
{
    public enum DeliveryStatus
    {
        Preparing = 1,     
        Packing = 2,       
        InTransit = 3,     
        Arrived = 4,       
        Confirmed = 5,    
        Accident = 6
    }
}
