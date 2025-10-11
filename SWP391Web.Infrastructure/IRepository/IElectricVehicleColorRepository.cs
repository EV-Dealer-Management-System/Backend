﻿using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IElectricVehicleColorRepository : IRepository<ElectricVehicleColor>
    {
        Task<ElectricVehicleColor?> GetByCodeAsync(string colorCode);
        Task<ElectricVehicleColor?> GetByNameAsync(string colorName);
        Task<ElectricVehicleColor?> GetByIdsAsync(Guid colorId);
        Task<List<ElectricVehicleColor?>> GetAvailableColorsByModelIdAndVersionIdAsync(Guid modelId, Guid versionId);
        Task<bool> IsColorExistsById(Guid colorId);
        Task<bool> IsColorExistsByName(string colorName);
        Task<bool> IsColorExistsByCode(string colorCode);

    }
}
