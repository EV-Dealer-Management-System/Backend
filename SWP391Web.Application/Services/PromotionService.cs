using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Promotion;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class PromotionService : IPromotionService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreatePromotionAsync(CreatePromotionDTO createPromotionDTO)
        {
            try
            {
                var isExistPromotion = await _unitOfWork.PromotionRepository.IsExistPromotionByNameAsync(createPromotionDTO.Name);
                if (isExistPromotion)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Promotion is exist"
                    };
                }
                if (createPromotionDTO.ModelId.HasValue)
                {
                    var existByModel = await _unitOfWork.PromotionRepository
                        .GetActivePromotionByModelIdAsync(createPromotionDTO.ModelId.Value);

                    if (existByModel != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "This model already has an active promotion."
                        };
                    }
                }

                if (createPromotionDTO.VersionId.HasValue)
                {
                    var existByVersion = await _unitOfWork.PromotionRepository
                        .GetActivePromotionByVersionIdAsync(createPromotionDTO.VersionId.Value);

                    if (existByVersion != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "This version already has an active promotion."
                        };
                    }
                }

                if (createPromotionDTO.StartDate >= createPromotionDTO.EndDate)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "StartDate must before EndDate",
                        StatusCode = 400
                    };
                }

                // Start Date and End Date can't in the past  
                if (createPromotionDTO.StartDate < DateTime.UtcNow || createPromotionDTO.EndDate < DateTime.UtcNow)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "StartDate and EndDate can't be in the past",
                        StatusCode = 400
                    };
                }

                if (createPromotionDTO.DiscountType == DiscountType.Percentage)
                {
                    if (createPromotionDTO == null || createPromotionDTO.Percentage < 0 || createPromotionDTO.Percentage > 100)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Percentage must in 0 to 100",
                            StatusCode = 400
                        };
                    }
                }
                else if (createPromotionDTO.DiscountType == DiscountType.FixAmount)
                {
                    if(createPromotionDTO.FixedAmount <= 0)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Fix Amount can't be lower than 0",
                            StatusCode = 400
                        };
                    }
                }

                Promotion promotion = new Promotion
                {
                    Name = createPromotionDTO.Name,
                    Description = createPromotionDTO.Description,
                    Percentage = createPromotionDTO?.Percentage,
                    FixedAmount = createPromotionDTO?.FixedAmount,
                    ModelId = createPromotionDTO?.ModelId,
                    VersionId = createPromotionDTO?.VersionId,
                    DiscountType = createPromotionDTO.DiscountType,
                    StartDate = createPromotionDTO.StartDate,
                    EndDate = createPromotionDTO.EndDate,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                if(promotion == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid promotion"
                    };
                }

                await _unitOfWork.PromotionRepository.AddAsync(promotion , CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Create promotion successfully",
                    Result = promotion
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message

                };
            }
        }

        public async Task<ResponseDTO> DeletePromotionAsync(Guid promotionId)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(promotionId);
                if (promotion == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Promotion not exist"
                    };
                }

                promotion.IsActive = false;
                _unitOfWork.PromotionRepository.Update(promotion);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Delete promotion successfully"
                };
            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetAllAsync()
        {
            try
            {
                var promotions = (await _unitOfWork.PromotionRepository.GetAllAsync())
                    .Where(p => p.IsActive == true);

                var getPromotion = _mapper.Map<List<GetPromotionDTO>>(promotions);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get all promotion successfully",
                    Result = getPromotion
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetPromotionByIdAsync(Guid promotionId)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(promotionId);
                if (promotion == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Promotion not exist"
                    };
                }

                var getPromotion = _mapper.Map<GetPromotionDTO>(promotion);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get promotion succesfully",
                    Result = getPromotion
                };
            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetPromotionByNameAsync(string name)
        {
            try
            {
                var promotions = await _unitOfWork.PromotionRepository.GetPromotionByNameAsync(name);
                if(promotions == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Promotion not found",
                        StatusCode = 404
                    };
                }

                var getPromotion = _mapper.Map<GetPromotionDTO>(promotions);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get promotion name successfully",
                    StatusCode = 201,
                    Result = getPromotion
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

        public async Task<ResponseDTO> UpdatePromotionAsync(Guid promotionId, UpdatePromotionDTO updatePromotionDTO)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(promotionId);
                if (promotion == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Promotion not exist"
                    };
                }

                //Validate Start & End Day
                if (updatePromotionDTO.StartDate.HasValue && updatePromotionDTO.EndDate.HasValue)
                {
                    if (updatePromotionDTO.StartDate >= updatePromotionDTO.EndDate)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Start Date must before End Date"
                        };
                    }

                    if (updatePromotionDTO.StartDate < DateTime.UtcNow || updatePromotionDTO.EndDate < DateTime.UtcNow)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Start Date and End Date must be in the future"
                        };
                    }
                }
                if (updatePromotionDTO.StartDate.HasValue && updatePromotionDTO.EndDate is null)
                {
                    if (updatePromotionDTO.StartDate > promotion.EndDate)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Start Date can not below End Date"
                        };
                    }
                }

                if (updatePromotionDTO.DiscountType == DiscountType.Percentage)
                {
                    if (updatePromotionDTO.Percentage is not null)
                    {
                        if (updatePromotionDTO.Percentage < 0 || updatePromotionDTO.Percentage > 100)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = "Percentage must be 0 to 100"
                            };
                        }
                    }
                }
                else if (updatePromotionDTO.DiscountType == DiscountType.FixAmount)
                {
                    if (updatePromotionDTO.FixedAmount == null || updatePromotionDTO.FixedAmount <= 0)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Fix Amount must over 0"
                        };
                    }
                }

                if (!string.IsNullOrEmpty(updatePromotionDTO.Name))
                {
                    var newName = updatePromotionDTO.Name.Trim().ToUpper();
                    if (newName != promotion.Name)
                    {
                        var isExist = await _unitOfWork.PromotionRepository.IsExistPromotionByNameExceptAsync(newName, promotion.Id);
                        if (isExist)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = "promotion is exist"
                            };
                        }
                        promotion.Name = newName;
                    }
                }

                // Update các trường khác nếu có
                if (!string.IsNullOrWhiteSpace(updatePromotionDTO.Description))
                {
                    promotion.Description = updatePromotionDTO.Description.Trim();
                }

                if (updatePromotionDTO.StartDate.HasValue && updatePromotionDTO.StartDate > DateTime.MinValue)
                {
                    promotion.StartDate = updatePromotionDTO.StartDate.Value;
                }

                if (updatePromotionDTO.EndDate.HasValue && updatePromotionDTO.EndDate > DateTime.MinValue)
                {
                    promotion.EndDate = updatePromotionDTO.EndDate.Value;
                }

                if (Enum.IsDefined(typeof(DiscountType), updatePromotionDTO.DiscountType))
                {
                    promotion.DiscountType = updatePromotionDTO.DiscountType;
                }

                if (updatePromotionDTO.DiscountType == DiscountType.Percentage && updatePromotionDTO.Percentage.HasValue)
                {
                    promotion.Percentage = updatePromotionDTO.Percentage;
                    promotion.FixedAmount = null; // Đặt FixedAmount thành null nếu PromotionType là Percentage
                }
                else if (updatePromotionDTO.FixedAmount.HasValue)
                {
                    promotion.FixedAmount = updatePromotionDTO.FixedAmount;
                    promotion.Percentage = null; // Đặt Percentage thành null nếu PromotionType là FixedAmount
                }

                _unitOfWork.PromotionRepository.Update(promotion);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Update promotion successfully",
                    Result = promotion
                };
            }
            
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }
    }
}
