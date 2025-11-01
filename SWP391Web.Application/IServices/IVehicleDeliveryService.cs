using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.VehicleDelivery;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IVehicleDeliveryService
    {
        Task<ResponseDTO> CreateVehicleDeliveryAsync(CreateVehicleDeliveryDTO createVehicleDeliveryDTO);
        Task<ResponseDTO> GetAllVehicleDelivery(DeliveryStatus? status = null);
        Task<ResponseDTO> GetVehicleDeliveryById(Guid deliveryId);
        Task<ResponseDTO> UpdateVehicleDeliveryStatus(ClaimsPrincipal user , Guid deliveryId , DeliveryStatus newStatus);
    }
}
