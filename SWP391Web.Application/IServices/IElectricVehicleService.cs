﻿using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IElectricVehicleService
    {
        Task<ResponseDTO> GetAllVehiclesAsync(ClaimsPrincipal user);
        Task<ResponseDTO> GetVehicleByIdAsync(Guid vehicleId);
        Task<ResponseDTO> GetVehicleByVinAsync(string vin);
        Task<ResponseDTO> CreateVehicleAsync(CreateElecticVehicleDTO createElectricVehicleDTO);
        Task<ResponseDTO> UpdateVehicleAsync(Guid vehicleId, UpdateElectricVehicleDTO updateElectricVehicleDTO);
        Task<ResponseDTO> UpdateVehicleStatusAsync(Guid vehicleId, ElectricVehicleStatus newStatus);
        Task<ResponseDTO> GetAvailableQuantityByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId);
        Task<ResponseDTO> GetDealerInventoryAsync(ClaimsPrincipal user);
        Task<ResponseDTO> GetSampleVehiclesAsync(ClaimsPrincipal user);

    }
}
