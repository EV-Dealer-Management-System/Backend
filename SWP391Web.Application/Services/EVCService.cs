using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EVC;
using SWP391Web.Application.IService;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class EVCService : IEVCService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public EVCService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ResponseDTO> CreateEVMStaff(CreateEVMStaffDTO createEVMStaffDTO)
        {
            try
            {
                var isEmailExist = await _unitOfWork.UserManagerRepository.IsEmailExist(createEVMStaffDTO.Email);
                if (isEmailExist)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Email already exists."
                    };
                }

                var user = new ApplicationUser
                {
                    Email = createEVMStaffDTO.Email,
                    UserName = createEVMStaffDTO.Email,
                    FullName = createEVMStaffDTO.FullName,
                    PhoneNumber = createEVMStaffDTO.PhoneNumber,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                var password = "EVMStaff@" + Guid.NewGuid().ToString()[..6].ToUpper();
                var result = await _unitOfWork.UserManagerRepository.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 500,
                        Message = "Failed to create EVM Staff."
                    };
                }

                await _unitOfWork.UserManagerRepository.AddToRoleAsync(user, StaticUserRole.EVMStaff);
                await _unitOfWork.SaveAsync();

                await _emailService.SendEmployeeAaccountEmail(createEVMStaffDTO.Email, createEVMStaffDTO.FullName, password);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "EVM Staff created successfully."
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
    }
}
