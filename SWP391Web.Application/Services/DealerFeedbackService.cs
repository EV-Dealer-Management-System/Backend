using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerFeedBackDTO;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
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
    public class DealerFeedbackService : IDealerFeedbackService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IS3Service _s3Service;

        public DealerFeedbackService(IUnitOfWork unitOfWork, IMapper mapper, IS3Service s3Service)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _s3Service = s3Service;
        }
        public async Task<ResponseDTO> CreateDealerFeedbackAsync(ClaimsPrincipal user, CreateDealerFeedBackDTO createDealerFeedBackDTO)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }
                
                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId,CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found ",
                        StatusCode = 404
                    };
                }

                DealerFeedback dealerFeedback = new DealerFeedback
                {
                    DealerId = dealer.Id,
                    FeedbackContent = createDealerFeedBackDTO.FeedbackContent,
                    Status = createDealerFeedBackDTO.Status,
                    CreatedAt = DateTime.UtcNow
                };
                if(dealerFeedback == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer feedback is null",
                        StatusCode = 404
                    };
                }

                //if(createDealerFeedBackDTO.Key != null && createDealerFeedBackDTO.Key.Any())
                //{
                //    foreach (var key in createDealerFeedBackDTO.Key)
                //    {
                //        var fileName = Path.GetFileName(key);
                //        dealerFeedback.DealerFBAttachments.Add(new DealerFBAttachment
                //        {
                //            FileName = fileName,
                //            Key = key
                //        });
                //    }
                //}

                await _unitOfWork.DealerFeedbackRepository.AddAsync(dealerFeedback, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Create dealer feedback successfully",
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
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
