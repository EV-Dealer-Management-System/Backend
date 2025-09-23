using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ContractRevision
    {
        public Guid Id { get; private set; }
        public int Version { get; private set; }
        public ContractStatus Status { get; private set; } = ContractStatus.Draft;

        public Guid ContractTemplateVerionId { get; private set; }
        public string HtmlSnapshot { get; private set; } = default!;
        public string DrafPdfKey { get; private set; } = default!;
        public string DraftSha256 { get; private set; } = default!;

        private ContractRevision() { }
        public ContractRevision(int version, Guid contractTemplateVerionId, string htmlSnapshot, string drafPdfKey, string draftSha256)
        {
            Id = Guid.NewGuid();
            Version = version;
            ContractTemplateVerionId = contractTemplateVerionId;
            HtmlSnapshot = htmlSnapshot;
            DrafPdfKey = drafPdfKey;
            DraftSha256 = draftSha256;
        }
    }
}
