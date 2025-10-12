using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.EContractTemplate
{
    public class CreateEContractTemplateDTO
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Html { get; set; } = null!;
    }
}
