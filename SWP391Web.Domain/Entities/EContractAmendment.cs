using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class EContractAmendment
    {
        public Guid Id { get; private set; }
        public Guid EContractId { get; private set; }
        public string? Title { get; private set; }
        public string HtmlKey { get; private set; } = null!;
        public string HtmlSha256 { get; private set; } = null!;
        public string? HtmlTag { get; private set; }
        public string? Notes { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = null!;

        public EContract EContract = null!;

        private EContractAmendment() { }

        public EContractAmendment(Guid eContractId,string? title, string htmlKey, string htmlSha256, string? htmlTag, string? notes, string createdBy)
        {
            EContractId = eContractId;
            Title = title;
            HtmlKey = htmlKey;
            HtmlSha256 = htmlSha256;
            HtmlTag = htmlTag;
            Notes = notes;
            CreatedBy = createdBy;
        }
    }
}
