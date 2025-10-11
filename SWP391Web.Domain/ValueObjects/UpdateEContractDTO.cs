using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.ValueObjects
{
    public class UpdateEContractDTO
    {
        [FromForm(Name = "Id")]       
        public string Id { get; set; } = null!;

        [FromForm(Name = "Subject")]  
        public string Subject { get; set; } = null!;

        [FromForm(Name = "file")]     
        public IFormFile File { get; set; } = null!;
    }
}
