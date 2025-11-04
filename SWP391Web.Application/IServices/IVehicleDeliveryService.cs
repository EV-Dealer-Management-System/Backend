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
        Task<ResponseDTO> GetAllVehicleDelivery(int pageNumber, int pageSize, DeliveryStatus? status, Guid? templateId, CancellationToken ct);
        Task<ResponseDTO> GetVehicleDeliveryById(Guid deliveryId, CancellationToken ct);
        Task<ResponseDTO> UpdateVehicleDeliveryStatus(ClaimsPrincipal user, Guid deliveryId, DeliveryStatus newStatus, CancellationToken ct, string? reason = null);
    }
}