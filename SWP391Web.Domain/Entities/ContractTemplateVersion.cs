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

        private ContractTemplateVersion() { }

        internal ContractTemplateVersion(int versionNo, string html, string? css, string createdBy, string? notes)
        {
            VersionNo = versionNo;
            ContentHtml = html;
            StyleCss = css;
            CreatedBy = createdBy;
            IsActive = false;
            Notes = notes;
        }

        internal void SetActive(bool active) => IsActive = active;
    }
}
