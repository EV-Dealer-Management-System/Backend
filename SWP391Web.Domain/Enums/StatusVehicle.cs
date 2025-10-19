using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Enums
{
    public enum StatusVehicle
    {
        Available = 1,
        Pending = 2,
        Booked = 3,
        InTransit = 4,
        Sold = 5,
        AtDealer = 6,
        Maintenance = 7,
    }
}
