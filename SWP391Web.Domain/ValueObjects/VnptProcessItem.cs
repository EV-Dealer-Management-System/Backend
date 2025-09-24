using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.ValueObjects
{
    public record VnptProcessItem(int OrderNo, string ProcessedByUserCode, string AccessPermissionCode);
}
