using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.EContract
{
    public class GetAccessTokenDTO
    {
        public string AccessToken { get; set; } = null!;
        public int UserId { get; set; }
    }
}
