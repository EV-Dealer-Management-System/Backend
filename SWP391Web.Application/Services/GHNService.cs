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
    public class GHNService : IGHNService
    {
        private readonly IGHNClient _ghnClient;
        public GHNService(IGHNClient ghnClient)
        {
            _ghnClient = ghnClient;
        }
        public async Task<ResponseDTO> GetProvincesAsync()
        {
            try
            {
                var response = await _ghnClient.GetProvincesAsync();

                if (response.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "Success to get province list",
                        Result = response.Data
                    };
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = response.Message ?? "Failed to get province list",
                        StatusCode = 400
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred while processing your request. {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
