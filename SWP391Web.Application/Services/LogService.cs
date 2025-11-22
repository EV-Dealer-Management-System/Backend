using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class LogService : ILogService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public LogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> AddLogAsync(ClaimsPrincipal user, LogType logType, string entityName, string? additionalInfo, string description, CancellationToken ct)
        {
            //var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var role = user.FindFirst(ClaimTypes.Role)?.Value;
            //if (userId == null)
            //{
            //    return new ResponseDTO
            //    {
            //        IsSuccess = false,
            //        Message = "User not found",
            //        StatusCode = 404,
                    
            //    };
            //}

            //Guid? dealerId = null;
            //if (role == StaticUserRole.DealerManager)
            //{
            //    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
            //    if(dealer == null)
            //    {
            //        return new ResponseDTO
            //        {
            //            IsSuccess = false,
            //            Message = "Dealer not found",
            //            StatusCode = 404
                        
            //        };
            //    }
            //    dealerId = dealer.Id;
            //}
            throw new NotImplementedException();



        }

        public Task<ResponseDTO> GetAllLogsAsync(ClaimsPrincipal user, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
