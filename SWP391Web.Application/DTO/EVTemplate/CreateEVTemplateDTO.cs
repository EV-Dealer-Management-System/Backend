using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.EVTemplate
{
    public class CreateEVTemplateDTO
    {
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Description { get; set; }
        public List<string>? AttachmentKeys { get; set; } = new();
    }
}
