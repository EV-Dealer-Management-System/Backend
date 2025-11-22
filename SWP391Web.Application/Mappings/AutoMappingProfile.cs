using Aspose.Words.XAttr;
using AutoMapper;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using SWP391Web.Application.DTO.Appointment;
using SWP391Web.Application.DTO.AppointmentSetting;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using SWP391Web.Application.DTO.BookingEVDetail;
using SWP391Web.Application.DTO.Customer;
using SWP391Web.Application.DTO.CustomerFeedback;
using SWP391Web.Application.DTO.CustomerOrder;
using SWP391Web.Application.DTO.Dealer;
using SWP391Web.Application.DTO.DealerDebt;
using SWP391Web.Application.DTO.DealerFeedBackDTO;
using SWP391Web.Application.DTO.DepositSetting;
using SWP391Web.Application.DTO.EContract;
using SWP391Web.Application.DTO.EContractTemplate;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.DTO.ElectricVehicleColor;
using SWP391Web.Application.DTO.ElectricVehicleModel;
using SWP391Web.Application.DTO.ElectricVehicleVersion;
using SWP391Web.Application.DTO.EVCInventory;
using SWP391Web.Application.DTO.EVTemplate;
using SWP391Web.Application.DTO.Log;
using SWP391Web.Application.DTO.Notification;
using SWP391Web.Application.DTO.OrderDetail;
using SWP391Web.Application.DTO.Payment;
using SWP391Web.Application.DTO.Promotion;
using SWP391Web.Application.DTO.Quote;
using SWP391Web.Application.DTO.QuoteDetail;
using SWP391Web.Application.DTO.VehicleDelivery;
using SWP391Web.Application.DTO.VehicleDeliveryDetail;
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
            .ForMember(d => d.QuoteDetails,
                opt => opt.MapFrom(s => s.Quote != null ? s.Quote.QuoteDetails : new List<QuoteDetail>()))
            .ForMember(d => d.Customer, opt => opt.MapFrom(s => s.Customer))
            .ForMember(d => d.OrderDetails, opt => opt.MapFrom(s => s.OrderDetails))
            .ForMember(d => d.Econtracts, opt => opt.MapFrom(s => s.EContracts)).ReverseMap();

            CreateMap<OrderDetail, GetOrderDetailDTO>()
            .ForMember(d => d.ElectricVehicle, opt => opt.MapFrom(s => s.ElectricVehicle)).ReverseMap();

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
                .ForMember(dest => dest.BookingEVDetails, opt => opt.MapFrom(src => src.BookingEVDetails))
                .ForMember(dest => dest.EContract, opt => opt.MapFrom(src => src.EContract)).ReverseMap();

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

            CreateMap<Quote, GetQuoteDTO>()
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

            CreateMap<Promotion, GetPromotionDTO>().ReverseMap();

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
                .ForMember(dest => dest.ManagerEmail, opt => opt.MapFrom(src => src.Manager.Email))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.DealerTier.Level)).ReverseMap();

            CreateMap<AppointmentSetting, GetAppointSettingDTO>().ReverseMap();

            CreateMap<Appointment, GetCreateAppointmentDTO>().ReverseMap();

            CreateMap<Appointment, GetAppointmentDTO>()
                .ForMember(dest => dest.EVTemplate, opt => opt.MapFrom(src => new ViewTemplate
                {
                    EVTemplateId = src.EVTemplate.Id,
                    VersionId = src.EVTemplate.VersionId,
                    VersionName = src.EVTemplate.Version.VersionName,
                    ModelId = src.EVTemplate.Version.ModelId,
                    ModelName = src.EVTemplate.Version.Model.ModelName,
                    ColorId = src.EVTemplate.ColorId,
                    ColorName = src.EVTemplate.Color.ColorName
                }))
                .ForMember(dest => dest.Dealer, opt => opt.MapFrom(src => new ViewDealerName
                {
                    DealerId = src.Dealer.Id,
                    DealerName = src.Dealer.Name
                }))
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => new ViewCustomerName
                {
                    CustomerId = src.Customer.Id,
                    CustomerName = src.Customer.FullName
                }))
                .ReverseMap();

            CreateMap<DepositSetting, GetDepositSettingDTO>()
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager.FullName))
                .ForMember(dest => dest.DealerName, opt => opt.MapFrom(src => src.Dealer.Name)).ReverseMap();

            CreateMap<DealerFeedback, GetDealerFeedBackDTO>()
                .ForMember(dest => dest.DealerName, opt => opt.MapFrom(src => src.Dealer.Name)).ReverseMap();

            CreateMap<CustomerFeedback, GetCustomerFeedbackDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.Email))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer.PhoneNumber))
                .ReverseMap();

            CreateMap<Notification, GetNotificationDTO>().ReverseMap();

            CreateMap<DealerTier, GetDealerTierDTO>().ReverseMap();

            CreateMap<DealerDebtTransaction, GetDealerDebtTransactionDTO>().ReverseMap();

            CreateMap<Transaction, GetTransactionDTO>().ReverseMap();
            
            CreateMap<VehicleDelivery, GetVehicleDeliveryDTO>()
                .ForMember(dest => dest.VehicleDeliveryDetails,opt => opt.MapFrom(src => src.VehicleDeliveryDetails))
                .ReverseMap();
                
            CreateMap<VehicleDeliveryDetail, GetVehicleDeliveryDetailDTO>()
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.ElectricVehicle.VIN));

            CreateMap<DealerDebt, GetDealerDebtDTO>().ReverseMap();

            CreateMap<DealerDailyInventory, DemandSeriesPointDTO>()
                .ForMember(dest => dest.Ds, opt => opt.MapFrom(src => src.SnapshotDate))
                .ForMember(dest => dest.Y, opt => opt.MapFrom(src => src.Outflow)).ReverseMap();

            CreateMap<DealerDailyInventory, ForecastTargetDTO>().ReverseMap();
            CreateMap<Log, GetLogDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.LogType, opt => opt.MapFrom(src => src.LogType.ToString()));
                
        }
    }
}
