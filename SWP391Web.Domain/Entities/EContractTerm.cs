using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class EContractTerm
    {
        public Guid Id { get; private set; }
        public string? Name { get; private set; }
        public int DealerLevel { get; private set; } // dealer level
        public string? RoleRepresentative { get; private set; } // signatory's name
        public string? RoleTitle { get; private set; } // title of the signatory on behalf of the company
        public int ExpiryYear { get; private set; } // number of years the contract is valid
        public string? Scope { get; private set; } // scope of the contract
        public string? Pricing { get; private set; } // pricing details
        public string? Payment { get; private set; } // payment terms
        public string? Commitment { get; private set; } // commitment details
        public string? Region { get; private set; } // region covered by the contract
        public string? NoticeDay { get; private set; } // number of days notice before price change
        public string? OrderConfirmDays { get; private set; } // number of days to confirm order
        public string? DeliveryLocation { get; private set; } // delivery location
        public string? PaymentMethod { get; private set; } // payment method
        public string? PaymentDueDays { get; private set; } // number of days payment is due
        public string? PenaltyRate { get; private set; } // penalty rate for late payment
        public string? ClaimDays { get; private set; } // deadline for reporting errors/returns
        public string? TerminationNoticeDays { get; private set; } // time to cure before termination
        public string? DisputeLocation { get; private set; } // location for dispute resolution
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } =  null!;
    }
}
