using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IService
{
    public interface ITokenService
    {
        Task<string> GenerateJwtAccessTokenAysnc(ApplicationUser user);
        Task<string> GenerateJwtRefreshTokenAsync(ApplicationUser user, bool rememberMe);
    }
}
