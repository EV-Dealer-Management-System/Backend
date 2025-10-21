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

        public async Task<ResponseDTO> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByEmailAsync(forgotPasswordDTO.Email);
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
                var resetLink = $"{StaticLinkUrl.WebUrl}/api/reset-password?userId={user.Id}&token={encodedToken}";

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

        public async Task<ResponseDTO> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByIdAsync(resetPasswordDTO.UserId);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "User not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                var decodedToken = Uri.UnescapeDataString(resetPasswordDTO.Token);
                var isSuccess = await _unitOfWork.UserManagerRepository.ResetPasswordAsync(user, decodedToken, resetPasswordDTO.Password);

                if (!isSuccess.Succeeded)
                {
                    return new ResponseDTO
                    {
                        Message = "Password reset failed, Token not correct",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                return new ResponseDTO
                {
                    Message = "Password reset successfully",
                    IsSuccess = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at ResetPassword in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> ChangePassword(ChangePasswordDTO changePasswordDTO, ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return new ResponseDTO
                    {
                        Message = "User not found",
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

                var isOldPasswordValid = await _unitOfWork.UserManagerRepository.CheckPasswordAsync(user, changePasswordDTO.CurrentPassword);
                if (!isOldPasswordValid)
                {
                    return new ResponseDTO
                    {
                        Message = "Current password is incorrect",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var isSuccess = await _unitOfWork.UserManagerRepository.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);
                if (!isSuccess.Succeeded)
                {
                    return new ResponseDTO
                    {
                        Message = "Change password failed",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                return new ResponseDTO
                {
                    Message = "Change password successfully",
                    IsSuccess = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at ChangePassword in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> HandleGoogleCallbackAsync(ClaimsPrincipal userClaims)
        {
            try
            {
                var email = userClaims.FindFirst(ClaimTypes.Email)?.Value;
                var name = userClaims.FindFirst(ClaimTypes.Name)?.Value;
                var googleSub = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(email))
                {
                    return new ResponseDTO { 
                        IsSuccess = false, 
                        StatusCode = 404, 
                        Message = "User not found in internal system."
                    };
                }

                var user = await _unitOfWork.UserManagerRepository.GetByEmailAsync(email);

                if (user is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "User not found"
                    };
                }
                var logins = await _unitOfWork.UserManagerRepository.HasLogin(user);
                var hasGoogleLinked = logins.Any(login => login.LoginProvider == "Google" && login.ProviderKey == googleSub);
                if (!hasGoogleLinked)
                {
                    var linkResult = await _unitOfWork.UserManagerRepository.AddLoginGoogleAsync(user);
                    if (!linkResult.Succeeded)
                    {
                        var msg = string.Join("; ", linkResult.Errors.Select(e => e.Description));
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 500,
                            Message = $"Failed to link Google account: {msg}"
                        };
                    }
                }

                var accessToken = await _tokenService.GenerateJwtAccessTokenAysnc(user);
                var refreshToken = await _tokenService.GenerateJwtRefreshTokenAsync(user, rememberMe: true);

                await _unitOfWork.SaveAsync();
                var getUser = _mapper.Map<GetApplicationUserDTO>(user);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Google login successful",
                    Result = new { 
                        AccessToken = 
                        accessToken, 
                        RefreshToken =
                        refreshToken, 
                        UserData = getUser
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error at HandleGoogleCallbackAsync: {ex.Message}"
                };
            }
        }

    }
}
