using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Dealer;
using SWP391Web.Application.IService;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
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
    public class DealerService : IDealerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public DealerService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task<ResponseDTO> CreateDealerStaffAsync(ClaimsPrincipal claimUser, CreateDealerStaffDTO createDealerStaffDTO, CancellationToken ct)
        {
            try
            {
                var userId = claimUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Users do not own any dealers"
                    };
                }

                var staff = await _unitOfWork.UserManagerRepository.GetByEmailAsync(createDealerStaffDTO.Email);
                ApplicationUser user;
                if (staff is null)
                {
                    user = new ApplicationUser
                    {
                        UserName = createDealerStaffDTO.Email,
                        Email = createDealerStaffDTO.Email,
                        FullName = createDealerStaffDTO.FullName,
                        PhoneNumber = createDealerStaffDTO.PhoneNumber,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };

                    var randomPassword = "Staff@" + Guid.NewGuid().ToString()[..6].ToUpper();
                    var result = await _unitOfWork.UserManagerRepository.CreateAsync(user, randomPassword);

                    if (!result.Succeeded)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 500,
                            Message = "Failed to create Dealer Staff."
                        };
                    }

                    await _emailService.SendDealerStaffAaccountEmail(createDealerStaffDTO.Email, createDealerStaffDTO.FullName, randomPassword, dealer.Name);
                }
                else
                {
                    var isActiveDealerMember = await _unitOfWork.DealerMemberRepository.IsActiveDealerMemberByEmailAsync(dealer.Id, createDealerStaffDTO.Email, ct);
                    if (isActiveDealerMember)
                    {
                        var dealerActive = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(staff.Id, ct);
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 409,
                            Message = $"The user is still active at dealer {dealerActive!.Name}."
                        };
                    }

                    var isEmailExist = await _unitOfWork.DealerMemberRepository.IsExistDealerMemberByEmailAsync(dealer.Id, createDealerStaffDTO.Email, ct);
                    if (isEmailExist)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 409,
                            Message = "Email staff already exists."
                        };
                    }

                    user = staff;
                    await _emailService.NotifyAddedToDealerExistingUser(createDealerStaffDTO.Email, createDealerStaffDTO.FullName, $"Nhân viên đại lý" , dealer.Name); // After can open more role
                }

                await _unitOfWork.UserManagerRepository.AddToRoleAsync(user, StaticUserRole.DealerStaff);

                var dealerMember = new DealerMember
                {
                    ApplicationUserId = user.Id,
                    DealerId = dealer.Id,
                    RoleInDealer = DealerRole.Staff,
                };
                await _unitOfWork.DealerMemberRepository.AddAsync(dealerMember, ct);

                await _unitOfWork.SaveAsync();
    
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Dealer Staff created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create new dealer staff at DealerService:  {ex.Message}"
                };
            }
        }

        public Task<ResponseDTO> GetAllDealerStaffAsync(string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending, int pageNumber, int PageSize, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
