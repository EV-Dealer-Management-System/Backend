using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.EContract
{
    public class GetEContractDTO
    {
        public Guid Id { get; private set; }
        public Guid TemplateId { get; private set; }
        public Guid TemplateVersionId { get; private set; }
        public int TemplateVersionNo { get; private set; }

        public EContractStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = null!;
        public string OwnerBy { get; private set; } = null!;
    }
}
