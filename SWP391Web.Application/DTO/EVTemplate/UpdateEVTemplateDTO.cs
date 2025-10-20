using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.EVTemplate
{
    public class UpdateEVTemplateDTO
    {
        public string? Description { get; set; }
        public List<string> AttachmentKeys { get; set; } = new();
    }
}
