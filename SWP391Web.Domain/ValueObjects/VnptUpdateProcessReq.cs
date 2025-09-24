using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.ValueObjects
{
    public record VnptUpdateProcessReq(string Id, bool ProcessInOrder, List<VnptProcessItem> Processes);
}
