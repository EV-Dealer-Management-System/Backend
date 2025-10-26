using AutoMapper;
using Microsoft.EntityFrameworkCore.Update.Internal;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DepositSetting;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System.Security.Claims;

namespace SWP391Web.Application.Services
{
    public class DepositSettingService : IDepositSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DepositSettingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> CreateUpdateDepositSetting(ClaimsPrincipal userClaim, decimal depositPercentage, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet."
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer not found for the manager."
                    };
                }

                var existingSetting = await _unitOfWork.DepositSettingRepository.GetByDealerIdAsync(dealer.Id, ct);
                if (existingSetting is null)
                {
                    var depositSetting = new DepositSetting
                    {
                        MaxDepositPercentage = depositPercentage,
                        ManagerId = userId,
                        DealerId = dealer.Id
                    };

                    await _unitOfWork.DepositSettingRepository.AddAsync(depositSetting, ct);
                }
                else
                {
                    existingSetting.MaxDepositPercentage = depositPercentage;
                    _unitOfWork.DepositSettingRepository.Update(existingSetting);
                }

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Deposit setting created/updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"An error occurred that create or update deposit setting at DepositSetiingService: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetDepositSetting(ClaimsPrincipal userClaim, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet."
                    };
                }

                var userRole = userClaim.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "User role not found."
                    };
                }


                DepositSetting? depositSetting;
                if (userRole.Equals(StaticUserRole.Admin))
                {
                    depositSetting = await _unitOfWork.DepositSettingRepository.GetByUserIdAsync(userId, ct);
                }
                else if (userRole.Equals(StaticUserRole.DealerManager))
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 404,
                            Message = "Dealer not found for the manager."
                        };
                    }

                    depositSetting = await _unitOfWork.DepositSettingRepository.GetByDealerIdAsync(dealer.Id, ct);
                    if (depositSetting is null)
                    {
                        depositSetting = await _unitOfWork.DepositSettingRepository.GetByDefaultAsync(ct);
                    }
                }
                else
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 404,
                            Message = "Dealer not found for the staff."
                        };
                    }

                    depositSetting = await _unitOfWork.DepositSettingRepository.GetByDealerIdAsync(dealer.Id, ct);
                    if (depositSetting is null)
                    {
                        depositSetting = await _unitOfWork.DepositSettingRepository.GetByDefaultAsync(ct);
                    }
                }

                if (depositSetting is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Deposit setting not found."
                    };
                }

                var getDeposit = _mapper.Map<GetDepositSettingDTO>(depositSetting);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Deposit setting retrieved successfully.",
                    Result = getDeposit
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"An error occurred that get deposit setting at DepositSettingService: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateAllSettings(ClaimsPrincipal userClaim, UpdateAllDepositSettingsDTO settingsDTO, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet."
                    };
                }

                var adminDepositSetting = await _unitOfWork.DepositSettingRepository.GetByUserIdAsync(userId, ct);
                if (adminDepositSetting is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Admin deposit setting not found."
                    };
                }

                if (settingsDTO.MinDepositPercentage < 0 || settingsDTO.MaxDepositPercentage > 100)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Admin deposit percentage must be between 0 and 100."
                    };
                }

                if (settingsDTO.MinDepositPercentage > settingsDTO.MaxDepositPercentage
                    || (settingsDTO.MinDepositPercentage > adminDepositSetting.MaxDepositPercentage && settingsDTO.MaxDepositPercentage is null)
                    || (settingsDTO.MaxDepositPercentage < adminDepositSetting.MinDepositPercentage && settingsDTO.MinDepositPercentage is null))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Admin min deposit percentage cannot be greater than max deposit percentage."
                    };
                }


                if (settingsDTO.MaxDepositPercentage is not null)
                {
                    adminDepositSetting.MaxDepositPercentage = settingsDTO.MaxDepositPercentage.Value;
                }

                if (settingsDTO.MinDepositPercentage is not null)
                {
                    adminDepositSetting.MinDepositPercentage = settingsDTO.MinDepositPercentage.Value;
                }

                _unitOfWork.DepositSettingRepository.Update(adminDepositSetting);

                var depositSettings = (await _unitOfWork.DepositSettingRepository.GetAllAsync()).Where(ds => ds.MaxDepositPercentage > adminDepositSetting.MaxDepositPercentage && ds.ManagerId != userId);
                foreach (var setting in depositSettings)
                {
                    setting.MaxDepositPercentage = adminDepositSetting.MaxDepositPercentage;
                    _unitOfWork.DepositSettingRepository.Update(setting);
                }

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "All deposit settings updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"An error occurred that update all deposit settings at DepositSetiingService: {ex.Message}"
                };
            }
        }
    }
}
