using SWP391Web.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IGHNClient
    {
        Task<GhnResult<List<GhnProvince>>> GetProvincesAsync(CancellationToken ct = default);
        Task<GhnResult<List<GhnDistrict>>> GetDistrictsAsync(int provinceId, CancellationToken ct = default);
        Task<GhnResult<List<GhnWard>>> GetWardsAsync(int districtId, CancellationToken ct = default);
    }
}
