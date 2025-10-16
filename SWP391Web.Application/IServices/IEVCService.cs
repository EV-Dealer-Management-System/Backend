using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.EVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IEVCService
    {
        Task<ResponseDTO> CreateEVMStaff(CreateEVMStaffDTO createEVMStaffDTO);
        Task<ResponseDTO> GetAllEVMStaff(string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending, int pageNumber, int pageSize);
    }
}
