using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface ILogService
    {
        Task<ResponseDTO> AddLogAsync(ClaimsPrincipal user, LogType logType, string entityName, string description, CancellationToken ct);
        Task<ResponseDTO> GetAllLogsAsync(ClaimsPrincipal user , int pageNumber , int pageSize, CancellationToken ct);
    }
}
