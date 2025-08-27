using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IService;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System.Security.Claims;

namespace SWP391Web.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AuthService(IUnitOfWork unitOfWork, IEmailService emailService, ITokenService tokenService, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Method to register a new customer
        public async Task<ResponseDTO> RegisterCustomer(RegisterCustomerDTO registerCustomerDTO)
        {
            try
            {
                var isEmailExist = await _unitOfWork.UserManagerRepository.IsEmailExist(registerCustomerDTO.Email);
                if (isEmailExist)
                {
                    return new ResponseDTO
                    {
                        Message = "Email already in use",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                ApplicationUser newUser = new ApplicationUser
                {
                    Email = registerCustomerDTO.Email,
                    UserName = registerCustomerDTO.Email,
                    FullName = registerCustomerDTO.FullName,
                };

                var result = await _unitOfWork.UserManagerRepository.CreateAsync(newUser, registerCustomerDTO.Password);
                if (!result.Succeeded)
                {
                    return new ResponseDTO
                    {
                        Message = "Registration failed",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                Customer newCustomer = new Customer
                {
                    UserId = newUser.Id
                };

                var addToRoleResult = await _unitOfWork.UserManagerRepository.AddToRoleAsync(newUser, StaticUserRole.Customer);
                if (addToRoleResult is null)
                {
                    return new ResponseDTO
                    {
                        Message = "Role assignment failed",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var addCustomerResult = await _unitOfWork.CustomerRepository.AddAsync(newCustomer);
                if (addCustomerResult is null)
                {
                    return new ResponseDTO
                    {
                        Message = "Customer creation failed",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var saveResult = await _unitOfWork.SaveAsync();
                if (saveResult <= 0)
                {
                    return new ResponseDTO
                    {
                        Message = "Database save failed",
                        IsSuccess = false,
                        StatusCode = 500
                    };
                }

                var isSendSuccess = await SendVerifyEmail(newUser);

                if (!isSendSuccess)
                {
                    return new ResponseDTO
                    {
                        Message = "Failed to send verification email",
                        IsSuccess = false,
                        StatusCode = 500
                    };
                }

                return new ResponseDTO
                {
                    Message = "Customer registered successfully",
                    IsSuccess = true,
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at RegisterCustomer in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }


        // Method to send verification email using ClaimsPrincipal to get current user, useful for resending verification email
        public async Task<ResponseDTO> ResendVerifyEmail(ClaimsPrincipal userClaim)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        Message = "User ID not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                var user = await _unitOfWork.UserManagerRepository.GetByIdAsync(userId);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "User not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                var isConfirmed = user.EmailConfirmed;
                if (isConfirmed)
                {
                    return new ResponseDTO
                    {
                        Message = "Email is already verified",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var token = await _unitOfWork.UserManagerRepository.GenerateEmailConfirmationTokenAsync(user);
                var verifyLink = $"https://localhost:7280/api/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await _emailService.SendVerifyEmail(user.Email, verifyLink);

                return new ResponseDTO
                {
                    Message = "Verification email sent successfully",
                    IsSuccess = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at SendVerifyEmail in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        // Overloaded method to send verification email directly using ApplicationUser, useful after registration
        private async Task<bool> SendVerifyEmail(ApplicationUser user)
        {
            var isSuccess = false;
            try
            {
                var token = await _unitOfWork.UserManagerRepository.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = Uri.EscapeDataString(token);
                var verifyLink = $"https://localhost:7280/api/verify-email?userId={user.Id}&token={encodedToken}";

                isSuccess = await _emailService.SendVerifyEmail(user.Email, verifyLink);
                return isSuccess;
            }
            catch (Exception ex)
            {
                return isSuccess;
            }
        }

        public async Task<ResponseDTO> VerifyEmil(string userId, string token)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByIdAsync(userId);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "User not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                var isConfirmed = user.EmailConfirmed;
                if (isConfirmed)
                {
                    return new ResponseDTO
                    {
                        Message = "Email is already verified",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var decodedToken = Uri.UnescapeDataString(token);

                var result = await _unitOfWork.UserManagerRepository.ConfirmEmailAsync(user, decodedToken);
                if (!result.Succeeded)
                {
                    return new ResponseDTO
                    {
                        Message = "Email verification failed",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                return new ResponseDTO
                {
                    Message = "Email verified successfully",
                    IsSuccess = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at VerifyEmail in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> LoginUser(LoginUserDTO loginUserDTO)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByEmailAsync(loginUserDTO.Email);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "Email is not exist",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    var remainingMinutes = (user.LockoutEnd.Value - DateTimeOffset.UtcNow).Minutes;
                    return new ResponseDTO
                    {
                        Message = $"Account is locked. Try again in {remainingMinutes} minutes.",
                        IsSuccess = false,
                        StatusCode = 403
                    };
                }

                var isPasswordValid = await _unitOfWork.UserManagerRepository.CheckPasswordAsync(user, loginUserDTO.Password);
                if (!isPasswordValid)
                {
                    await _unitOfWork.UserManagerRepository.AccessFailedAsync(user);
                    return new ResponseDTO
                    {
                        Message = $"Password is incorrect. If you enter {5 - user.AccessFailedCount} incorrectly again, your account will be locked for 5 minutes.",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                if (!user.EmailConfirmed)
                {
                    return new ResponseDTO
                    {
                        Message = "Email is not verified",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var accessToken = await _tokenService.GenerateJwtAccessTokenAysnc(user);
                var refreshToken = await _tokenService.GenerateJwtRefreshTokenAsync(user, loginUserDTO.RememberMe);

                var getUser = _mapper.Map<GetApplicationUserDTO>(user);

                return new ResponseDTO
                {
                    Message = "Login successful",
                    IsSuccess = true,
                    StatusCode = 200,
                    Result = new
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        UserData = getUser
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at LoginUser in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> ForgotPassword(string email)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByEmailAsync(email);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "Email is not exist",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var token = await _unitOfWork.UserManagerRepository.GeneratePasswordResetTokenAsync(user);

                var encodedToken = Uri.EscapeDataString(token);
                var resetLink = $"https://localhost:7280/api/reset-password?userId={user.Id}&token={encodedToken}";

                var isSendSuccess = await _emailService.SendResetPassword(user.Email, resetLink);
                if (!isSendSuccess)
                {
                    return new ResponseDTO
                    {
                        Message = "Failed to send reset password email",
                        IsSuccess = false,
                        StatusCode = 500
                    };
                }

                return new ResponseDTO
                {
                    Message = "Reset password email sent successfully",
                    IsSuccess = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at ForgotPassword in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }
    }
}
