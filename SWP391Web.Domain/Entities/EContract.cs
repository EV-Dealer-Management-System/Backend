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
        public Guid TemplateId { get; private set; }
        public string? Name { get; set; }
        // Snapshot
        public string? SnapshotKey { get; private set; }
        public EContractStatus Status { get; private set; } = EContractStatus.Draft;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = null!;
        public string OwnerBy { get; private set; } = null!;

        public ApplicationUser Owner { get; private set; } = null!;

        private EContract() { }
        public EContract(Guid id, Guid templateId, string? name, string createdBy, string ownerBy, EContractStatus status)
        {
            Id = id;
            TemplateId = templateId;
            Name = name;
            Status = status;
            CreatedBy = createdBy;
            OwnerBy = ownerBy;
        }

        public void UpdateStatus(EContractStatus status)
        {
            Status = status;
        }

        public void UpdateSnapshotKey(string snapshotKey)
        {
            SnapshotKey = snapshotKey;
        }

        public EContractTemplate EContractTemplate = null!;
    }
}
