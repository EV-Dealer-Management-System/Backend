using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DisCount;
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
    public class DiscountService : IDiscountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DiscountService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateDiscountAsync(CreateDiscountDTO createDiscountDTO)
        {
            try
            {
                var isExistedDiscount = await _unitOfWork.DiscountRepository.IsExistByCode(createDiscountDTO.Code);
                if (isExistedDiscount is true)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Discount code is existed",
                    };
                }

                if (createDiscountDTO.StartDate >= createDiscountDTO.EndDate)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Start date must be before end date",
                    };
                }

                // StartDate and EndDate must be in the future
                if (createDiscountDTO.StartDate < DateTime.UtcNow || createDiscountDTO.EndDate < DateTime.UtcNow)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Start date and end date must be in the future",
                    };
                }

                if (createDiscountDTO.DiscountType == DiscountType.Percentage)
                {
                    if (createDiscountDTO.Percentage is null || createDiscountDTO.Percentage < 0 || createDiscountDTO.Percentage > 100)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Percentage must be between 0 and 100",
                        };
                    }

                }
                else if (createDiscountDTO.DiscountType == DiscountType.FixedAmount)
                {
                    if (createDiscountDTO.FixedAmount < 0)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Fixed amount must be greater than 0",
                        };
                    }
                }

                Discount discount = new Discount
                {
                    Code = createDiscountDTO.Code,
                    DiscountType = createDiscountDTO.DiscountType,
                    Percentage = createDiscountDTO.Percentage,
                    FixedAmount = createDiscountDTO.FixedAmount,
                    Description = createDiscountDTO.Description,
                    StartDate = createDiscountDTO.StartDate,
                    EndDate = createDiscountDTO.EndDate
                };

                if (discount is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Create discount failed",
                    };
                }
                await _unitOfWork.DiscountRepository.AddAsync(discount);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Create discount successfully",
                    Result = discount
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO> DeleteDiscountAsync(Guid discountId)
        {
            try
            {
                var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(discountId);
                if (discount is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Discount not found",
                    };
                }

                discount.IsActive = false;
                _unitOfWork.DiscountRepository.Update(discount);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Delete discount successfully",
                    Result = discount
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO> GetAllDiscounts(string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending)
        {
            try
            {
                var discounts = (await _unitOfWork.DiscountRepository.GetAllAsync())
                    .Where(d => d.IsActive == true);
                if (!string.IsNullOrEmpty(filterOn) && !string.IsNullOrEmpty(filterQuery))
                {
                    string filter = filterOn.ToLower();
                    string query = filterQuery.ToUpper();

                    discounts = filter switch
                    {
                        "code" => discounts.Where(d => d.Code.ToUpper().Contains(query, StringComparison.CurrentCulture)),

                        _ => discounts
                    };
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    discounts = sortBy.Trim().ToLower() switch
                    {
                        "code" => isAcsending is true ?
                            discounts.OrderBy(d => d.Code) :
                            discounts.OrderByDescending(d => d.Code),
                        "startdate" => isAcsending is true ?
                            discounts.OrderBy(d => d.StartDate) :
                            discounts.OrderByDescending(d => d.StartDate),
                        "enddate" => isAcsending is true ?
                            discounts.OrderBy(d => d.EndDate) :
                            discounts.OrderByDescending(d => d.EndDate),
                        "createdat" => isAcsending is true ?
                            discounts.OrderBy(d => d.CreatedAt) :
                            discounts.OrderByDescending(d => d.CreatedAt),
                        _ => discounts
                    };
                }

                var getDiscountDTOs = _mapper.Map<List<GetDiscountDTO>>(discounts);

                if (discounts is null || !discounts.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "No discounts found",
                    };
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get all discounts successfully",
                    Result = discounts
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO> GetDiscountByCode(string code)

        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Code is required",
                    };
                }

                var discount = await _unitOfWork.DiscountRepository.GetByCodeAsync(code);
                if (discount is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Discount not found",
                    };
                }

                var getDiscountDTO = _mapper.Map<GetDiscountDTO>(discount);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get discount by code successfully",
                    Result = getDiscountDTO
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }
        public async Task<ResponseDTO> GetDiscountByIdAsync(Guid id)
        {
            try
            {
                var discount = _unitOfWork.DiscountRepository.GetByIdAsync(id);
                if (discount is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Discount not found",
                    };
                }

                var getDiscountDTO = _mapper.Map<GetDiscountDTO>(discount);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get discount by id successfully",
                    Result = getDiscountDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO> UpdateDiscountAsync(Guid discountId, UpdateDiscountDTo updateDiscountDTO)
        {
            try
            {
                var discount = await _unitOfWork.DiscountRepository.GetByIdAsync(discountId);
                if (discount is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Discount not found",
                    };
                }

                if (updateDiscountDTO.StartDate.HasValue && updateDiscountDTO.EndDate.HasValue)
                {
                    if (updateDiscountDTO.StartDate >= updateDiscountDTO.EndDate)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Start date must be before end date",
                        };
                    }

                    if (updateDiscountDTO.StartDate < DateTime.UtcNow || updateDiscountDTO.EndDate < DateTime.UtcNow)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Start date and end date must be in the future",
                        };
                    }
                }

                    if (updateDiscountDTO.StartDate.HasValue && updateDiscountDTO.EndDate is null)
                    {
                        if (updateDiscountDTO.StartDate > discount.EndDate)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = "Start date must be before end date",
                            };
                        }
                    }

                    if (updateDiscountDTO.DiscountType == DiscountType.Percentage)
                    {
                        if (updateDiscountDTO.Percentage is not null)
                        {
                            if (updateDiscountDTO.Percentage < 0 || updateDiscountDTO.Percentage > 100)
                            {
                                return new ResponseDTO
                                {
                                    IsSuccess = false,
                                    StatusCode = 400,
                                    Message = "Percentage must be between 0 and 100",
                                };
                            }
                        }
                    }
                    else if (updateDiscountDTO.DiscountType == DiscountType.FixedAmount)
                    {
                        if (updateDiscountDTO.FixedAmount is not null && updateDiscountDTO.Percentage <= 0)
                        {
                                return new ResponseDTO
                                {
                                    IsSuccess = false,
                                    StatusCode = 400,
                                    Message = "Fixed amount must be greater than 0",
                                };
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(updateDiscountDTO.Code))
                    {
                        var newCode = updateDiscountDTO.Code.Trim().ToUpper();
                        if (newCode != discount.Code)
                        {
                            var isExistedCode = await _unitOfWork.DiscountRepository.IsExistByCodeExceptId(newCode, discount.Id);
                            if (isExistedCode)
                            {
                                return new ResponseDTO
                                {
                                    IsSuccess = false,
                                    StatusCode = 400,
                                    Message = "Discount code is existed",
                                };
                            }
                            discount.Code = newCode;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(updateDiscountDTO.Description))
                    {
                        discount.Description = updateDiscountDTO.Description.Trim();
                    }

                    if (updateDiscountDTO.StartDate.HasValue && updateDiscountDTO.StartDate > DateTime.MinValue)
                    {
                        discount.StartDate = updateDiscountDTO.StartDate.Value;
                    }

                    if (updateDiscountDTO.EndDate.HasValue && updateDiscountDTO.EndDate > DateTime.MinValue)
                    {
                        discount.EndDate = updateDiscountDTO.EndDate.Value;
                    }

                    if (Enum.IsDefined(typeof(DiscountType), updateDiscountDTO.DiscountType))
                    {
                        discount.DiscountType = updateDiscountDTO.DiscountType;
                    }

                    if (updateDiscountDTO.DiscountType == DiscountType.Percentage && updateDiscountDTO.Percentage.HasValue)
                    {
                        discount.Percentage = updateDiscountDTO.Percentage.Value;
                        discount.FixedAmount = null; //Fix amount will be null if percentage is updated
                    }
                    else if (updateDiscountDTO.DiscountType == DiscountType.FixedAmount && updateDiscountDTO.FixedAmount.HasValue)
                    {
                        discount.FixedAmount = updateDiscountDTO.FixedAmount.Value;
                        discount.Percentage = null; //Percentage will be null if fix amount is updated
                    }

                    _unitOfWork.DiscountRepository.Update(discount);
                    await _unitOfWork.SaveAsync();

                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "Update discount successfully",
                        Result = discount
                    };
                
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }
    }
}
