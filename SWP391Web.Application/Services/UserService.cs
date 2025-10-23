using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Customer;
using SWP391Web.Application.IService;
using SWP391Web.Application.IServices;
using SWP391Web.Infrastructure.IRepository;
using System.Security.Claims;

namespace SWP391Web.Application.Services
{
    public class UserService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> GetUserProfile(ClaimsPrincipal user)
        {
            try
            {
                var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Not login yet"
                    };
                }

                var customer = await _unitOfWork.UserManagerRepository.GetByIdAsync(userId);
                if (customer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer not found"
                    };
                }

                var getUser = _mapper.Map<GetCustomerDTO>(customer);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get customer profile successfully",
                    Result = getUser
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
