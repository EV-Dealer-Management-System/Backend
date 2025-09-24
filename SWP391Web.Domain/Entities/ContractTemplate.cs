using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ContractTemplate
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        private readonly List<ContractTemplateVersion> _versions = new();
        public IReadOnlyCollection<ContractTemplateVersion> Versions => _versions.AsReadOnly();

        private ContractTemplate() { }
        public ContractTemplate(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public ContractTemplateVersion PublistNewVersion(string contentHtml, string? styleCss, string createdBy, string? notes = null)
        {
            var newVersionNo = _versions.Any() ? _versions.Max(v => v.VersionNo) + 1 : 1;
            var newVersion = new ContractTemplateVersion(newVersionNo, contentHtml, styleCss, createdBy, notes);
            _versions.Add(newVersion);
            return newVersion;
        }

        public ContractTemplateVersion? GetActive() => _versions.Where(v => v.IsActive).OrderByDescending(v => v.VersionNo).FirstOrDefault();

        public void Active(int versionNo)
        {
            foreach(var v in _versions)
            {
                v.SetActive(v.VersionNo == versionNo);
            }
        }
    }
}
