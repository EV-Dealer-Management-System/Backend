using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Repository
{
    public class VehicleDeliveryRepository : Repository<VehicleDelivery>, IVehicleDeliveryRepository
    {
        public readonly ApplicationDbContext _context;
        public VehicleDeliveryRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<VehicleDelivery?> GetVehicleDeliveryById(Guid deliveryId, CancellationToken ct)
        {
            return await _context.VehicleDeliveries.FirstOrDefaultAsync(vd => vd.Id == deliveryId,ct);
        }

        public Task<VehicleDelivery?> VehicleDeliveryByBookingId(Guid BookingId, CancellationToken ct)
        {
            return _context.VehicleDeliveries
                .Include(vd => vd.BookingEV)
                .FirstOrDefaultAsync(vd => vd.BookingEVId == BookingId,ct);
        }
    }
}
