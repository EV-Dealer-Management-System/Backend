using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Options
{
    public sealed class JwtOptions
    {
        public string ValidAudience { get; set; } = "";
        public string ValidIssuer { get; set; } = "";
        public string Secret { get; set; } = "";

        public RefreshTokenExpirationOptions RefreshTokenExpiration { get; set; } = new();
        public int AccessTokenExpiration { get; set; }       
        public int RefreshTokenExpirationRememberMe { get; set; }   
    }

    public sealed class RefreshTokenExpirationOptions
    {
        public int RememberMe { get; set; }
        public int Normal { get; set; }
    }
}
