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
        public Guid TemplateVersionId { get; private set; }
        public int TemplateVersionNo { get; private set; }

        // Snapshot
        public string? BaseHtmlKey { get; private set; }
        public string? FinalPdfKey { get; private set; }
        public string? ManifestKey { get; private set; }

        public string? BaseHtmlSha256 { get; private set; }
        public string? FinalPdfSha256 { get; private set; }
        public string? ManifestSha256 { get; private set; }
        public string? BaseHtmlTag { get; private set; }
        public string? FinalPdfTag { get; private set; }
        public string? ManifestTag { get; private set; }

        public EContractStatus Status { get; private set; } = EContractStatus.Draft;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = null!;
        public string OwnerBy { get; private set; } = null!;


        public readonly ApplicationUser Ower = null!;

        private readonly List<EContractAmendment> _amendments = new();
        public IReadOnlyCollection<EContractAmendment> Amendments => _amendments.AsReadOnly();

        private EContract() { }
        public EContract(Guid Id, EContractTemplate contractTemplate, EContractTemplateVersion templateVersion, string createdBy)
        {
            if (!templateVersion.IsActive)
                throw new InvalidOperationException("Only active template version can be used to create contract.");

            this.Id = Id;
            TemplateId = contractTemplate.Id;
            TemplateVersionId = templateVersion.Id;
            TemplateVersionNo = templateVersion.VersionNo;

            CreatedBy = createdBy;
        }
        public EContract(Guid Id, EContractTemplate contractTemplate, EContractTemplateVersion templateVersion, string? baseHtmlKey, string? manifestKey, string? baseHtmlSha256, string? manifestSha256, string? baseHtmlTag, string? manifestTag, string createdBy)
        {
            if (!templateVersion.IsActive)
                throw new InvalidOperationException("Only active template version can be used to create contract.");

            this.Id = Id;
            TemplateId = contractTemplate.Id;
            TemplateVersionId = templateVersion.Id;
            TemplateVersionNo = templateVersion.VersionNo;

            BaseHtmlKey = baseHtmlKey;
            ManifestKey = manifestKey;
            BaseHtmlSha256 = baseHtmlSha256;
            ManifestSha256 = manifestSha256;
            BaseHtmlTag = baseHtmlTag;
            ManifestTag = manifestTag;

            CreatedBy = createdBy;
        }

        public EContractAmendment AddAmendment(Guid eContractId, string title, string s3HtmlKey, string htmlSha256, string? htmlTag, string? notes, string createdBy)
        {
            if (Status is not (EContractStatus.Draft or EContractStatus.Ready))
                throw new InvalidOperationException("Only draft or sent contract can be amended.");

            var amendment = new EContractAmendment(eContractId, title, s3HtmlKey, htmlSha256, htmlTag, notes, createdBy);
            _amendments.Add(amendment);
            return amendment;
        }
    }
}
