using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Enums
{
    public enum EContractStatus
    {
        Draft = 0,
        Sent = 1,
        LockedForSigning = 2,
        Completed = 3,
        Cancelled = 4
    }
}
