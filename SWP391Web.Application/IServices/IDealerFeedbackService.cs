using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerFeedBackDTO;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IDealerFeedbackService
    {
        Task<ResponseDTO> CreateDealerFeedbackAsync(ClaimsPrincipal user ,CreateDealerFeedBackDTO createDealerFeedBackDTO);
        Task<ResponseDTO> GetAllDealerFeedbacksAsync(ClaimsPrincipal user , CancellationToken ct);
        Task<ResponseDTO> GetDealerFeedbackByIdAsync(Guid feedbackId);
        Task<ResponseDTO> UpdateDealerFeedbackStatusAsync(ClaimsPrincipal user, Guid feedbackId, FeedbackStatus newStatus);

    }
}
