using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Enums
{
    public enum ResultCode
    {
        Default = 0,
        NeedOtpConfirmation = 100,
        Unauthorized = 401
    }
}
