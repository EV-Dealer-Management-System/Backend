using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class EContract
    {
        public Guid Id { get; private set; }
        public string HtmlTemaple { get; private set; }
        public string? Name { get; set; }
        public EContractStatus Status { get; private set; } = EContractStatus.Draft;
        public EcontractType Type { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = null!;
        public string OwnerBy { get; private set; } = null!;

        public ApplicationUser Owner { get; private set; } = null!;

        private EContract() { }
        public EContract(Guid id, string htmlTemaple, string? name, string createdBy, string ownerBy, EContractStatus status, EcontractType type)
        {
            Id = id;
            HtmlTemaple = htmlTemaple;
            Name = name;
            Status = status;
            CreatedBy = createdBy;
            OwnerBy = ownerBy;
            Type = type;
        }

        public void UpdateStatus(EContractStatus status)
        {
            Status = status;
        }
    }
}
