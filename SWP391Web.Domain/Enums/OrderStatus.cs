using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Enums
{
    public enum OrderStatus
    {
        FullPending = 0,
        DepositPending = 1,
        ConfirmPending = 2,
        Confirmed = 3,
        Depositing = 4,
        Completed = 5,
        Cancelled = 6,
        Rejected = 7
    }
}
