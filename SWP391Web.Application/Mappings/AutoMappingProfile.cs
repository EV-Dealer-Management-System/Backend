using Aspose.Words.XAttr;
using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using SWP391Web.Application.DTO.BookingEVDetail;
using SWP391Web.Application.DTO.Customer;
using SWP391Web.Application.DTO.CustomerOrder;
using SWP391Web.Application.DTO.Dealer;
using SWP391Web.Application.DTO.EContract;
using SWP391Web.Application.DTO.EContractTemplate;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.DTO.ElectricVehicleColor;
using SWP391Web.Application.DTO.ElectricVehicleModel;
using SWP391Web.Application.DTO.ElectricVehicleVersion;
using SWP391Web.Application.DTO.EVCInventory;
using SWP391Web.Application.DTO.EVTemplate;
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
            CreateMap<Customer, GetCustomerDTO>().ReverseMap();
            CreateMap<CustomerOrder, GetCustomerOrderDTO>()
                .ForMember(dest => dest.QuoteDetails, opt => opt.MapFrom(src => src.Quote.QuoteDetails)).ReverseMap();
            CreateMap<ElectricVehicleColor, GetElectricVehicleColorDTO>().ReverseMap();
            CreateMap<ElectricVehicleModel, GetElectricVehicleModelDTO>().ReverseMap();
            CreateMap<ElectricVehicleVersion, GetElectricVehicleVersionDTO>().ReverseMap();
            CreateMap<ElectricVehicle, GetElecticVehicleDTO>()
                .ForMember(dest => dest.ElectricVehicleTemplate, opt => opt.MapFrom(src => new ViewTemplate
                {
                    EVTemplateId = src.ElectricVehicleTemplate.Id,
                    VersionId = src.ElectricVehicleTemplate.VersionId,
                    VersionName = src.ElectricVehicleTemplate.Version.VersionName,
                    ModelId = src.ElectricVehicleTemplate.Version.ModelId,
                    ModelName = src.ElectricVehicleTemplate.Version.Model.ModelName
                }))
                .ForMember(dest => dest.Warehouse, opt => opt.MapFrom(src => new ViewWarehouse
                {
                    WarehouseId = src.Warehouse.Id,
                    Name = src.Warehouse.WarehouseName,
                }));
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
            CreateMap<EContract, GetEContractDTO>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FullName)).ReverseMap();

            CreateMap<EContractTemplate, GetEContractTemplateDTO>().ReverseMap();
            CreateMap<Quote,GetQuoteDTO>()
                .ForMember(dest => dest.QuoteDetails, opt => opt.MapFrom(src => src.QuoteDetails)).ReverseMap();
            CreateMap<QuoteDetail, GetQuoteDetailDTO>()
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => new ViewColorName
                {
                    ColorId = src.ColorId,
                    ColorName = src.ElectricVehicleColor.ColorName
                }))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => new ViewVersionName
                {
                    VersionId = src.VersionId,
                    VersionName = src.ElectricVehicleVersion.VersionName,
                    ModelId = src.ElectricVehicleVersion.Model.Id,
                    ModelName = src.ElectricVehicleVersion.Model.ModelName,
                }))
                .ForMember(dest => dest.Promotion, opt => opt.MapFrom(src => new ViewPromotionName
                {
                    PromotionId = src.PromotionId,
                    PromotionName = src.Promotion.Name,
                }));
            CreateMap<Promotion,GetPromotionDTO>().ReverseMap();
            CreateMap<DealerMember, GetDealerStaffDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ApplicationUser.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.ApplicationUser.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.ApplicationUser.PhoneNumber)).ReverseMap();
            CreateMap<ElectricVehicleTemplate, GetEVTemplateDTO>()
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => new ViewColorName
                {
                    ColorId = src.Color.Id,
                    ColorName = src.Color.ColorName,
                }))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => new ViewVersionName
                {
                    VersionId = src.Version.Id,
                    VersionName = src.Version.VersionName,
                    ModelId = src.Version.Model.Id,
                    ModelName = src.Version.Model.ModelName,
                }));
                //.ForMember(dest => dest.ImgUrl, opt => opt.MapFrom(src => src.EVAttachments.Select(a => a.Key).ToList()));
            CreateMap<ElectricVehicle, GetVehicleByBookingDTO>()
                .ForMember(dest => dest.ElectricVehicleId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => new ViewColorName
                {
                    ColorId = src.ElectricVehicleTemplate.Color.Id,
                    ColorName = src.ElectricVehicleTemplate.Color.ColorName,
                }))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => new ViewVersionName
                {
                    VersionId = src.ElectricVehicleTemplate.Version.Id,
                    VersionName = src.ElectricVehicleTemplate.Version.VersionName,
                    ModelId = src.ElectricVehicleTemplate.Version.Model.Id,
                    ModelName = src.ElectricVehicleTemplate.Version.Model.ModelName,
                }))
                .ForMember(dest => dest.Warehouse, opt => opt.MapFrom(src => new ViewWarehouse
                {
                    WarehouseId = src.Warehouse.Id,
                    Name = src.Warehouse.WarehouseName,
                }));

            CreateMap<Dealer, GetDealerDTO>()
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager.FullName))
                .ForMember(dest => dest.ManagerEmail, opt => opt.MapFrom(src => src.Manager.Email)).ReverseMap();
        }
    }
}
