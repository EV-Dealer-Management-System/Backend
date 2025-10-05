using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class EContractTemplate
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        private readonly List<EContractTemplateVersion> _versions = new();
        public IReadOnlyCollection<EContractTemplateVersion> Versions => _versions.AsReadOnly();

        private EContractTemplate() { }
        public EContractTemplate(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public EContractTemplateVersion PublishNewVersion(string contentHtml, string? styleCss, string createdBy, string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(contentHtml)) throw new ArgumentException("contentHtml is required", nameof(contentHtml));
            if (string.IsNullOrWhiteSpace(createdBy)) throw new ArgumentException("createdBy is required", nameof(createdBy));

            var newVersionNo = _versions.Any() ? _versions.Max(v => v.VersionNo) + 1 : 1;
            var v = new EContractTemplateVersion(newVersionNo, contentHtml, styleCss, createdBy, notes);
            _versions.Add(v);

            v.Publish();    
            Active(v.VersionNo);
            return v;
        }

        public EContractTemplateVersion? GetActive() => _versions.Where(v => v.IsActive).OrderByDescending(v => v.VersionNo).FirstOrDefault();

        public void Active(int versionNo)
        {
            var target = _versions.SingleOrDefault(v => v.VersionNo == versionNo)
                         ?? throw new InvalidOperationException($"Version {versionNo} not found.");

            if (target.Status != TemplateVersionStatus.Published)
                throw new InvalidOperationException("Only published version can be activated.");

            foreach (var v in _versions)
            {
                v.SetActive(v.VersionNo == versionNo);
            }
        }

        public void SoftDelete() => IsDeleted = true;
    }
}
