using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO
{
    public class CreateDealerDTO
    {
        public string DealerName { get; set; } = null!;
        public string DealerAddress { get; set; } = null!;
        public string? DealerEmail { get; set; }
        public string? DealerPhoneNumber { get; set; }
        public string FullNameManager { get; set; } = null!;
        public string EmailManager { get; set; } = null!;
        public string? PhoneNumberManager { get; set; }
    }
}
