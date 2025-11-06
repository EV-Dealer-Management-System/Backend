using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IVehicleDeliveryRepository : IRepository<VehicleDelivery>
    {
        Task<VehicleDelivery?> GetVehicleDeliveryById(Guid deliveryId , CancellationToken ct);
        Task<VehicleDelivery?> GetVehicleDeliveryByBookingId(Guid BookingId , CancellationToken ct);
        Task<int> CountByDealerIdAsync(Guid dealerId, CancellationToken ct);
    }
}
