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
            CreateMap<Customer, GetCustomerDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.User.Sex))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth)).ReverseMap();
        }
    }
}
