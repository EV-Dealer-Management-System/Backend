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
    public class VehicleDeliveryDetailRepository : Repository<VehicleDeliveryDetail>, IVehicleDeliveryDetailRepository
    {
        public readonly ApplicationDbContext _context;
        public VehicleDeliveryDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<VehicleDeliveryDetail?> GetVehicleDeliveryDetailById(Guid detailId)
        {
            return await _context.VehicleDeliveryDetails.FirstOrDefaultAsync(vdd => vdd.Id == detailId);
        }
    }
}
