using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Constants
{
    public class StaticUserRole
    {
        public const string Admin = "Admin";
        public const string DealerManager = "DealerManager";
        public const string DealerStaff = "DealerStaff";
        public const string EVMStaff = "EVMStaff";
        public const string Admin_EVMStaff = Admin + "," + EVMStaff;
    }
}
