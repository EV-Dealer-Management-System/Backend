using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.Auth
{
    public class AuthResultDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public GetApplicationUserDTO UserData { get; set; } = new GetApplicationUserDTO();
    }
}
