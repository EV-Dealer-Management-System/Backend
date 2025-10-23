using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Repository
{
    public class BookingEVRepository : Repository<BookingEV>, IBookingEVRepository
    {
        public readonly ApplicationDbContext _context;
        public BookingEVRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<BookingEV>> GetAllBookingWithDetailAsync()
        {
            {
                return await _context.BookingEVs
                    .Include(b => b.BookingEVDetails)
                        .ThenInclude(bd => bd.Version)
                            .ThenInclude(v => v.Model)
                    .Include(b => b.BookingEVDetails)
                        .ThenInclude(bd => bd.Color)
                    .Include(b => b.Dealer)
                    .ToListAsync();
            }
        }

        public async Task<BookingEV?> GetBookingWithIdAsync(Guid bookingId)
        {
          return await _context.BookingEVs
                .Include(b => b.BookingEVDetails)
                    .ThenInclude(bd => bd.Version)
                        .ThenInclude(v => v.Model)
                .Include(b => b.BookingEVDetails)
                    .ThenInclude(bd => bd.Color)
                .Include(b => b.Dealer)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }
        public async Task<List<ElectricVehicle?>> GetVehiclesByBookingIdAsync(Guid bookingId)
        {
            var bookingDetails = await _context.BookingEVDetails
                .Where(bd => bd.BookingId == bookingId)
                .ToListAsync();

            var vehicles = new List<ElectricVehicle>();

            foreach (var detail in bookingDetails)
            {
                var matchedVehicles = await _context.ElectricVehicles
                    .Include(ev => ev.ElectricVehicleTemplate)
                        .ThenInclude(t => t.Version)
                            .ThenInclude(v => v.Model)
                    .Include(ev => ev.ElectricVehicleTemplate.Color)
                    .Include(ev => ev.Warehouse)
                    .Where(ev =>
                        ev.Status == ElectricVehicleStatus.Pending &&
                        ev.ElectricVehicleTemplate.VersionId == detail.VersionId &&
                        ev.ElectricVehicleTemplate.ColorId == detail.ColorId)
                    .OrderBy(ev => ev.ImportDate)
                    .Take(detail.Quantity)
                    .ToListAsync();

                vehicles.AddRange(matchedVehicles);
            }

            return vehicles;
        }


        public async Task<bool> IsBookingExistsById(Guid bookingId)
        {
            return await _context.BookingEVs.AnyAsync(b => b.Id == bookingId);
        }
    }
}
