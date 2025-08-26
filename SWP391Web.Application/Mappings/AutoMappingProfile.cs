using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Customer;
using SWP391Web.Domain.Entities;

namespace SWP391Web.Application.Mappings
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<ApplicationUser, GetApplicationUserDTO>().ReverseMap();
        }
    }
}
