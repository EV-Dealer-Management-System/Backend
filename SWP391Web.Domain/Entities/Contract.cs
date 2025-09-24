using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Contract
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public int ActiveVersion { get; private set; }

        private readonly List<ContractRevision> _revisions = new();
        public IReadOnlyCollection<ContractRevision> Revisions => _revisions.AsReadOnly();

        private Contract() { }
        public Contract(Guid orderId)
        {
            OrderId = orderId;
        }

        public void AddRevision(ContractRevision revision)
        {
            _revisions.Add(revision);
            ActiveVersion = revision.Version;
        }
    }
}
