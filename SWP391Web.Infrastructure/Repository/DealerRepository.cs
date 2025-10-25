﻿using Microsoft.EntityFrameworkCore;
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
    public class DealerRepository : Repository<Dealer>, IDealerRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerRepository(ApplicationDbContext context) : base(context)
        {
            this._context = context;
        }

        public async Task<Dealer?> GetByIdAsync(Guid dealerId, CancellationToken ct)
        {
            return await _context.Dealers
                .Include(dl => dl.Warehouse)
                .Where(dl => dl.Id == dealerId).FirstOrDefaultAsync(ct);
        }

        public async Task<Dealer?> GetDealerByUserIdAsync(string userId, CancellationToken ct)
        {
            return await _context.Dealers
                .AsNoTracking()
                .Include(dl => dl.Warehouse)
                .Where(dl => dl.DealerMembers.Any(dm => dm.ApplicationUserId == userId && dm.IsActive == true))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Dealer?> GetDealerByManagerIdAsync(string managerId, CancellationToken ct)
        {
            return await _context.Dealers
                .AsNoTracking()
                .Include(dl => dl.Manager)
                .Where(dl => dl.ManagerId == managerId)
                .FirstOrDefaultAsync(ct);
        }

        public Task<ApplicationUser?> GetManagerByDealerId(Guid dealerId, CancellationToken ct)
        {
            return _context.Dealers
                .AsNoTracking()
                .Where(dl => dl.Id == dealerId)
                .Select(dl => dl.Manager)
                .SingleOrDefaultAsync(ct);
        }

        public async Task<Dealer?> GetManagerByUserIdAsync(string userId, CancellationToken ct)
        {
           return await _context.Dealers.FirstOrDefaultAsync(dl => dl.ManagerId == userId, ct);

        }

        public async Task<bool> IsExistByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.Dealers
                .AnyAsync(dl => dl.Id == id, ct);
        }

        public async Task<bool> IsExistByNameAsync(string name, CancellationToken ct)
        {
            return await _context.Dealers
                .AnyAsync(dl => dl.Name == name, ct);
        }

        public async Task<Dealer?> GetDealerByManagerOrStaffAsync(string userdId, CancellationToken ct)
        {
            return await _context.Dealers
                .AsNoTracking()
                .Include(dl => dl.Manager)
                .Where(dl => dl.ManagerId == userdId
                        || dl.DealerMembers.Any(dm => dm.ApplicationUserId == userdId && dm.IsActive))
                .FirstOrDefaultAsync();
        }

        public async Task<Dealer?> GetTrackedDealerByManagerOrStaffAsync(string userId, CancellationToken ct)
        {
            return await _context.Dealers
                .Include(dl => dl.Customers)
                .Include(dl => dl.Manager)
                .Where(dl => dl.ManagerId == userId
                        || dl.DealerMembers.Any(dm => dm.ApplicationUserId == userId && dm.IsActive))
                .FirstOrDefaultAsync(ct);
        }
    }
}
