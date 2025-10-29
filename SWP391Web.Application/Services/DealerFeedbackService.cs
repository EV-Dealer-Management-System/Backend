using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerFeedBackDTO;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class DealerFeedbackService : IDealerFeedbackService
    {
        public Task<ResponseDTO> CreateDealerFeedbackAsync(ClaimsPrincipal user, CreateDealerFeedBackDTO createDealerFeedBackDTO)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDTO> GetAllDealerFeedbacksAsync(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDTO> GetDealerFeedbackByIdAsync(ClaimsPrincipal user, Guid feedbackId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDTO> UpdateDealerFeedbackStatusAsync(ClaimsPrincipal user, Guid feedbackId, FeedbackStatus newStatus)
        {
            throw new NotImplementedException();
        }
    }
}
