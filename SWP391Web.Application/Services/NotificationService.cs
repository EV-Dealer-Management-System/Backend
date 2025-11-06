using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Notification;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public async Task<ResponseDTO> GetAllNotification(ClaimsPrincipal userClaim, int pageNumber, int pageSize, CancellationToken ct)
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
                Expression<Func<Notification, bool>>? filter = null;
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

                    filter = n => n.DealerId == dealer.Id && n.TargetRole == StaticUserRole.DealerStaff;
                }
                else
                {
                    filter = n => n.DealerId == dealer.Id && n.TargetRole == StaticUserRole.DealerManager;
                }

                (IReadOnlyList<Notification> items, int total) result;
                result = await _unitOfWork.NotificationRepository.GetPagedAsync(
                            filter: filter,
                            includes: null,
                            orderBy: dm => dm.CreatedAt,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);

                var getNotication = _mapper.Map<List<GetNotificationDTO>>(result.items);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get notifications successfully",
                    Result = new
                    {
                        data = getNotication,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / pageSize)
                        }
                    },
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
