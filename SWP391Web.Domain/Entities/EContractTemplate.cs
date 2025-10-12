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
        public string ContentHtml { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        private EContractTemplate() { }
        public EContractTemplate(string code, string name, string contentHtml)
        {
            Id = Guid.NewGuid();
            Code = code;
            Name = name;
            ContentHtml = contentHtml;
        }

        public ICollection<EContract> EContracts { get; set; } = new List<EContract>();

    }
}
