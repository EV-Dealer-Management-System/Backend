using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using SWP391Web.Application.DTO.BookingEVDetail;
using SWP391Web.Application.DTO.Customer;
using SWP391Web.Application.DTO.EContract;
using SWP391Web.Application.DTO.EContractTemplate;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.DTO.ElectricVehicleColor;
using SWP391Web.Application.DTO.ElectricVehicleModel;
using SWP391Web.Application.DTO.ElectricVehicleVersion;
using SWP391Web.Application.DTO.EVCInventory;
using SWP391Web.Application.DTO.Promotion;
using SWP391Web.Application.DTO.Quote;
using SWP391Web.Application.DTO.QuoteDetail;
using SWP391Web.Application.DTO.Warehouse;
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
            CreateMap<ElectricVehicleColor, GetElectricVehicleColorDTO>().ReverseMap();
            CreateMap<ElectricVehicleModel, GetElectricVehicleModelDTO>().ReverseMap();
            CreateMap<ElectricVehicleVersion, GetElectricVehicleVersionDTO>().ReverseMap();
            CreateMap<ElectricVehicle, GetElecticVehicleDTO>().ReverseMap();
            CreateMap<BookingEV, GetBookingEVDTO>()
                .ForMember(dest => dest.BookingEVDetails, opt => opt.MapFrom(src => src.BookingEVDetails)).ReverseMap();
            CreateMap<BookingEVDetail, GetBookingEVDetailDTO>()
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => new VersionDTO
                {
                    VersionId = src.VersionId,
                    ModelId = src.Version.ModelId
                }));
            CreateMap<EVCInventory, GetEVCInventoryDTO>().ReverseMap();
            CreateMap<Warehouse, GetWarehouseDTO>().ReverseMap();
            CreateMap<EContract, GetEContractDTO>().ReverseMap();
            CreateMap<EContractTemplate, GetEContractTemplateDTO>().ReverseMap();
            CreateMap<Quote,GetQuoteDTO>().ReverseMap();
            CreateMap<QuoteDetail,GetQuoteDetailDTO>().ReverseMap();
            CreateMap<Promotion,GetPromotionDTO>().ReverseMap();
        }
    }
}
