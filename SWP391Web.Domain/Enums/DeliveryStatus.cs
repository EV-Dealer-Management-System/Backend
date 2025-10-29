using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Enums
{
    public enum DeliveryStatus
    {
        Preparing = 0,     
        Packing = 1,       
        InTransit = 2,     
        Arrived = 3,       
        Confirmed = 4,     
        Accident = 5
    }
}
