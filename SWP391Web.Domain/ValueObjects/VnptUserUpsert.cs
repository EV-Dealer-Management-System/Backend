using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.ValueObjects
{
    public record VnptUserUpsert(
        string Code,
        string UserName,
        string Name,
        string Email,
        string Phone,
        int ReceiveOtpMethod,              // 0: SMS, 1: Email, 2: Cả 2 (tùy hệ thống)
        int ReceiveNotificationMethod,     // 0: Email, ...
        int SignMethod,                    // 1: Draw, 2: SmartCA, ...
        bool SignConfirmationEnabled,
        bool GenerateSelfSignedCertEnabled,
        int Status                         // 1: Active
    );
}
