using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.CustomerFeedback;
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
    public interface ICustomerFeedbackService 
    {
        Task<ResponseDTO> CreateCustomerFeedbackAsync(ClaimsPrincipal user, CreateCustomerFeedbackDTO createCustomerFeedbackDTO);
        Task<ResponseDTO> GetAllCustomerFeedbacksAsync(ClaimsPrincipal user, CancellationToken ct);
        Task<ResponseDTO> GetCustomerFeedbackByIdAsync(ClaimsPrincipal user, Guid feedbackId);
        Task<ResponseDTO> UpdateCustomerFeedbackStatusAsync(ClaimsPrincipal user, Guid feedbackId, FeedbackStatus newStatus);
    }
}
