using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ContractTemplateVersion
    {
        public Guid Id { get; set; }
        public int VersionNo { get; set; }
        public string ContentHtml { get; set; } = null!;
        public string? StyleCss { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TemplateVersionStatus Status { set; get; } = TemplateVersionStatus.Draft;

        private ContractTemplateVersion() { }

        internal ContractTemplateVersion(int versionNo, string html, string? css, string createdBy, string? notes)
        {
            VersionNo = versionNo;
            ContentHtml = html;
            StyleCss = css;
            CreatedBy = createdBy;
            Notes = notes;
        }

        internal void Publish()
        {
            if (Status != TemplateVersionStatus.Draft)
                throw new InvalidOperationException("Only draft version can be published.");
            Status = TemplateVersionStatus.Published;
        }

        internal void SetActive(bool active)
        {
            if (Status != TemplateVersionStatus.Published && active)
                throw new InvalidOperationException("Only published version can be activated.");
            IsActive = active;
        }
    }
}
