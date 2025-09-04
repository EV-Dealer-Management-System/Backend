using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashBoardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetTotalCustomerAsync()
        {
            try
            {
                var totalCustomer = (await _unitOfWork.CustomerRepository.GetAllAsync(includeProperties: "User"))
                    .Where(c => c.User.LockoutEnabled).Count();
                return new ResponseDTO
                    {
                    IsSuccess = true,
                    Message = "Get total customer successfully",
                    Result = totalCustomer,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Can't get total customer: {ex.Message}",
                    Result = null,
                    StatusCode = 500
                };
            }
        }
    }
}
