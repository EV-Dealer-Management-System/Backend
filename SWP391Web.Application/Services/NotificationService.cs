using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Notification;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> GetAllNotification(ClaimsPrincipal userClaim, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = userClaim.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }
                }

                var notifications = (await _unitOfWork.NotificationRepository.GetAllAsync()).Where(n => n.DealerId == dealer.Id && n.TargetRole == role)
                    .OrderBy(n => n.CreatedAt).Take(10);

                var getNotication = _mapper.Map<List<GetNotificationDTO>>(notifications);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get notifications successfully",
                    Result = getNotication,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to get all notification: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
