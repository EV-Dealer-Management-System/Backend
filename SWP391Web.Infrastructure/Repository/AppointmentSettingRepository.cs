using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Repository
{
    public class AppointmentSettingRepository : IAppointmentSettingRepository
    {
        private static readonly List<AppointmentSetting> _settings = new()
        {
            new AppointmentSetting
            {
                Id = Guid.NewGuid(),
                DealerId = null, 
                AllowOverlappingAppointments = false,
                MaxConcurrentAppointments = 1,
                OpenTime = new TimeSpan(8, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0),
                MinIntervalBetweenAppointments = 60,
                CreatedAt = DateTime.UtcNow
            }
        };

        public Task<AppointmentSetting> AddAsync(AppointmentSetting entity, CancellationToken token)
        {
            _settings.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<AppointmentSetting?> GetByDealerIdAsync(Guid dealerId)
        {
            var setting = _settings.FirstOrDefault(s => s.DealerId == dealerId);
            return Task.FromResult(setting);
        }

        public Task<AppointmentSetting?> GetDefaultAsync(Guid id)
        {
            var setting = _settings.FirstOrDefault(s => s.DealerId == null);
            return Task.FromResult(setting);
        }

        public void Update(AppointmentSetting entity)
        {
            var existing = _settings.FirstOrDefault(s => s.Id == entity.Id);
            if (existing != null)
            {
                _settings.Remove(existing);
                _settings.Add(entity);
            }
        }

        public void Remove(AppointmentSetting entity)
        {
            _settings.Remove(entity);
        }

        public Task<IEnumerable<AppointmentSetting>> GetAllAsync(
            Expression<Func<AppointmentSetting, bool>>? filter = null,
            Func<IQueryable<AppointmentSetting>, IQueryable<AppointmentSetting>>? includes = null,
            Func<IQueryable<AppointmentSetting>, IOrderedQueryable<AppointmentSetting>>? orderBy = null,
            CancellationToken ct = default,
            bool asNoTracking = true)
        {
            var query = _settings.AsQueryable();
            if (filter != null)
                query = query.Where(filter);
            return Task.FromResult(query.AsEnumerable());
        }

        public Task AddRangeAsync(IEnumerable<AppointmentSetting> entities) => throw new NotImplementedException();
        public Task<AppointmentSetting?> GetAsync(Expression<Func<AppointmentSetting, bool>> filter, string? includeProperties = null) => throw new NotImplementedException();
        public Task<(IReadOnlyList<AppointmentSetting> Items, int Total)> GetPagedAsync<TKey>(Expression<Func<AppointmentSetting, bool>>? filter, Func<IQueryable<AppointmentSetting>, IQueryable<AppointmentSetting>>? includes, Expression<Func<AppointmentSetting, TKey>> orderBy, bool ascending, int pageNumber, int pageSize, CancellationToken ct = default, bool asNoTracking = true) => throw new NotImplementedException();
        public IQueryable<AppointmentSetting> Query(Expression<Func<AppointmentSetting, bool>>? filter = null, Func<IQueryable<AppointmentSetting>, IQueryable<AppointmentSetting>>? includes = null, bool asNoTracking = true) => throw new NotImplementedException();
        public void RemoveRange(IEnumerable<AppointmentSetting> entities) => throw new NotImplementedException();
        public void UpdateRange(IEnumerable<AppointmentSetting> entities) => throw new NotImplementedException();
    }
}
