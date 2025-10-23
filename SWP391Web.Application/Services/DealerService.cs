using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SWP391Web.Application.Services
{
    public class DealerService : IDealerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        public DealerService(IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
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
                    await _emailService.NotifyAddedToDealerExistingUser(createDealerStaffDTO.Email, createDealerStaffDTO.FullName, $"Nhân viên đại lý", dealer.Name); // After can open more role
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

        public async Task<ResponseDTO> GetAllDealerAsync(string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending, int pageNumber, int PageSize, CancellationToken ct)
        {
            try
            {
                Expression<Func<Dealer, bool>> baseFilter = d => d.DealerStatus == DealerStatus.Active;

                if (!string.IsNullOrWhiteSpace(filterOn) && (!string.IsNullOrWhiteSpace(filterQuery)))
                {
                    var query = filterQuery.Trim().ToLower();
                    baseFilter = filterOn.Trim().ToLower() switch
                    {
                        "name" => d => d.DealerStatus == DealerStatus.Active &&
                                       d.Name != null &&
                                       d.Name.ToLower().Contains(query),

                        _ => d => d.DealerStatus == DealerStatus.Active
                    };
                }

                string sortField = (sortBy ?? "createdat").Trim().ToLower();
                bool asc = isAcsending ?? true;

                (IReadOnlyList<Dealer> items, int total) result = (new List<Dealer>(), 0);
                Func<IQueryable<Dealer>, IQueryable<Dealer>> includes = q => q.Include(dm => dm.Manager);

                switch (sortField)
                {
                    case "name":
                        result = _unitOfWork.DealerRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: d => d.Name!,
                            ascending: asc,
                            pageNumber: pageNumber,
                            pageSize: PageSize,
                            ct: ct).Result;
                        break;

                    default:
                        result = _unitOfWork.DealerRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: d => d.Id,
                            ascending: asc,
                            pageNumber: pageNumber,
                            pageSize: PageSize,
                            ct: ct).Result;
                        break;
                }

                var data = _mapper.Map<List<GetDealerDTO>>(result.items);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get dealers successfully.",
                    Result = new
                    {
                        Data = data,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = PageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / PageSize)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get all dealers at DealerService: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetAllDealerStaffAsync(ClaimsPrincipal claimUser, string? filterOn, string? filterQuery, string? sortBy, bool? isAscending, int pageNumber, int pageSize, CancellationToken ct)
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
                var dealer = _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct).Result;
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Users do not own any dealers"
                    };
                }

                Expression<Func<DealerMember, bool>> baseFilter = dm => dm.DealerId == dealer.Id;


                if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
                {
                    var q = filterQuery.Trim().ToLower();
                    baseFilter = filterOn.Trim().ToLower() switch
                    {
                        "fullname" => dm => dm.DealerId == dealer.Id &&
                                            dm.ApplicationUser.FullName != null &&
                                            dm.ApplicationUser.FullName.ToLower().Contains(q),
                        "email" => dm => dm.DealerId == dealer.Id &&
                                         dm.ApplicationUser.Email != null &&
                                         dm.ApplicationUser.Email.ToLower().Contains(q),
                        _ => dm => dm.DealerId == dealer.Id
                    };
                }

                Func<IQueryable<DealerMember>, IQueryable<DealerMember>> includes =
                    q => q.Include(dm => dm.ApplicationUser);

                string sortField = (sortBy ?? "createdat").Trim().ToLower();
                bool asc = isAscending ?? true;

                (IReadOnlyList<DealerMember> items, int total) result;

                switch (sortField)
                {
                    case "fullname":
                        result = await _unitOfWork.DealerMemberRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: dm => dm.ApplicationUser.FullName!,
                            ascending: asc,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);
                        break;

                    case "email":
                        result = await _unitOfWork.DealerMemberRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: dm => dm.ApplicationUser.Email!,
                            ascending: asc,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);
                        break;

                    default:
                        result = await _unitOfWork.DealerMemberRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: dm => dm.ApplicationUser.CreatedAt,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);
                        break;
                }

                var data = _mapper.Map<List<GetDealerStaffDTO>>(result.items);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get dealer staffs successfully.",
                    Result = new
                    {
                        Data = data,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / pageSize)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get all dealer staffs at DealerService:  {ex.Message}"
                };
            }
        }
    }
}
