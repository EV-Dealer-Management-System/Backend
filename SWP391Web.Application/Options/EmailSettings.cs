using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Options
{
    public class EmailSettings
    {
        public string FromEmail { get; set; } = "";
        public string Host { get; set; } = "";
        public int Port { get; set; } = 25;
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
    }
}
