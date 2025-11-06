using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.Dashboard
{
    public class GetDealerManagerDBDTO
    {
        public string DealerName { get; set; } = null!;
        public int TotalBookings { get; set; }
        public int TotalDeliveries { get; set; }
        public int TotalQuotes { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalActiveStaff { get; set; }
    }
}
