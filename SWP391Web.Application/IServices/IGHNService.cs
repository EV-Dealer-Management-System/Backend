using SWP391Web.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IGHNService
    {
        Task<ResponseDTO> GetProvincesAsync();
        Task<ResponseDTO> GetDistrictsAsync(int provinceId);
        Task<ResponseDTO> GetWardsAsync(int districtId);
    }
}
