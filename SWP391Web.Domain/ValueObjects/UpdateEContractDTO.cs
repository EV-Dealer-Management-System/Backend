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
        [FromForm(Name = "Id")]        // nếu VNPT cần "id" thường, đổi Name="id"
        public string Id { get; set; } = null!;

        [FromForm(Name = "Subject")]   // BẮT BUỘC: VNPT đang báo lỗi field này
        public string Subject { get; set; } = null!;

        [FromForm(Name = "file")]      // field file đúng là "file"
        public IFormFile File { get; set; } = null!;
    }
}
