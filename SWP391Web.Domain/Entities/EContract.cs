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
        public string BaseHtmlKey { get; private set; } = null!;
        public string? FinalPdfKey { get; private set; }
        public string ManifestKey { get; private set; } = null!;

        public string BaseHtmlSha256 { get; private set; } = null!;
        public string? FinalPdfSha256 { get; private set; }
        public string? ManifestSha256 { get; private set; }
        public string? BaseHtmlTag { get; private set; }
        public string? FinalPdfTag { get; private set; }
        public string? ManifestTag { get; private set; }

        public EContractStatus Status { get; private set; } = EContractStatus.Draft;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = null!;

        private readonly List<EContractAmendment> _amendments = new();
        public IReadOnlyCollection<EContractAmendment> Amendments => _amendments.AsReadOnly();

        private EContract() { } 
        public EContract(ContractTemplate contractTemplate, ContractTemplateVersion templateVersion, string baseHtmlKey, string manifestKey, string baseHtmlSha256, string manifestSha256, string? baseHtmlTag, string? manifestTag, string createdBy)
        {
            if (!templateVersion.IsActive)
                throw new InvalidOperationException("Only active template version can be used to create contract.");

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

        public EContractAmendment AddAmendment(string title, string s3HtmlKey, string htmlSha256, string? htmlTag, string? notes, string createdBy)
        {
            if (Status is not (EContractStatus.Draft or EContractStatus.Sent))
                throw new InvalidOperationException("Only draft or sent contract can be amended.");

            var amendment = new EContractAmendment(title, s3HtmlKey, htmlSha256, htmlTag, notes, createdBy);
            _amendments.Add(amendment);
            return amendment;
        }

        public void MarkSent()
        {
            if (Status != EContractStatus.Draft)
                throw new InvalidOperationException("Only draft contract can be sent.");
            Status = EContractStatus.Sent;
        }

        public void LockForSigning(string finalPdfKey, string finalPdfSha256, string? finalPdfTag)
        {
            if (Status != EContractStatus.Sent)
                throw new InvalidOperationException("Send before locking.");
            Status = EContractStatus.LockedForSigning;
        }

        public void AttachFinalPdf(string finalPdfKey, string finalPdfSha256, string? finalPdfTag)
        {
            if (Status != EContractStatus.LockedForSigning)
                throw new InvalidOperationException("Only locked contract can attach final PDF.");
            FinalPdfKey = finalPdfKey;
            FinalPdfSha256 = finalPdfSha256;
            FinalPdfTag = finalPdfTag;
        }

        public void MarkCompleted()
        {
            if (Status != EContractStatus.LockedForSigning)
                throw new InvalidOperationException("Only locked contract can be completed.");
            Status = EContractStatus.Completed;
        }
    }
}
