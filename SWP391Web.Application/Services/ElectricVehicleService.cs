using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class ElectricVehicleService : IElectricVehicleService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public ElectricVehicleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseDTO> CreateVehicleAsync(CreateElecticVehicleDTO createElectricVehicleDTO)
        {
            try
            {
                var isVinExist = await _unitOfWork.ElectricVehicleRepository
                    .IsVehicleExistsByVIN(createElectricVehicleDTO.VIN);
                if (isVinExist)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Vehicle VIN already exists.",
                        StatusCode = 404
                    };
                }

                ElectricVehicle electricVehicle = new ElectricVehicle
                {
                    WarehouseId = createElectricVehicleDTO.WarehouseId,
                    VersionId = createElectricVehicleDTO.VersionId,
                    ColorId = createElectricVehicleDTO.ColorId,
                    VIN = createElectricVehicleDTO.VIN,
                    Status = createElectricVehicleDTO.Status,
                    ManufactureDate = createElectricVehicleDTO.ManufactureDate,
                    ImportDate = createElectricVehicleDTO.ImportDate,
                    WarrantyExpiryDate = createElectricVehicleDTO.WarrantyExpiryDate,
                    CurrentLocation = createElectricVehicleDTO.CurrentLocation,
                    CostPrice = createElectricVehicleDTO.CostPrice,
                    ImageUrl = createElectricVehicleDTO.ImageUrl,
                };
                if (electricVehicle is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Vehicle is null.",
                        StatusCode = 404
                    };
                }

                await _unitOfWork.ElectricVehicleRepository.AddAsync(electricVehicle, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Create vehicle successfully.",
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAllVehiclesAsync()
        {
            try
            {
                var vehicles = await _unitOfWork.ElectricVehicleRepository.GetAllAsync();
                var getVehicles = _mapper.Map<List<GetElecticVehicleDTO>>(vehicles);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get all vehicles successfully.",
                    StatusCode = 200,
                    Result = getVehicles
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleByIdAsync(Guid vehicleId)
        {
            try
            {
                var vehicle = await _unitOfWork.ElectricVehicleRepository.GetByIdsAsync(vehicleId);
                if (vehicle is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Vehicle not found.",
                        StatusCode = 404
                    };
                }
                var getVehicle = _mapper.Map<GetElecticVehicleDTO>(vehicle);

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get vehicle successfully.",
                    StatusCode = 200,
                    Result = getVehicle
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleByVinAsync(string vin)
        {
            try
            {
                var vehicle = await _unitOfWork.ElectricVehicleRepository.GetByVINAsync(vin);
                if (vehicle is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Vehicle not found.",
                        StatusCode = 404
                    };
                }

                var getVehicle = _mapper.Map<GetElecticVehicleDTO>(vehicle);

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Get vehicle successfully.",
                    StatusCode = 200,
                    Result = getVehicle
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = "Internal server error.",
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpdateVehicleAsync(Guid vehicleId, UpdateElectricVehicleDTO dto)
        {
            try
            {
                var vehicle = await _unitOfWork.ElectricVehicleRepository.GetByIdsAsync(vehicleId);
                if (vehicle == null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Vehicle not found.",
                        StatusCode = 404
                    };

                // Chỉ update các trường thông tin, không động đến khóa ngoại
                if (!string.IsNullOrWhiteSpace(dto.VIN))
                    vehicle.VIN = dto.VIN;

                if (dto.Status.HasValue)
                    vehicle.Status = dto.Status.Value;

                if (dto.ManufactureDate.HasValue && dto.ManufactureDate.Value != default)
                    vehicle.ManufactureDate = dto.ManufactureDate.Value;

                if (dto.ImportDate.HasValue && dto.ImportDate.Value != default)
                    vehicle.ImportDate = dto.ImportDate.Value;

                if (dto.WarrantyExpiryDate.HasValue && dto.WarrantyExpiryDate.Value != default)
                    vehicle.WarrantyExpiryDate = dto.WarrantyExpiryDate.Value;

                if (dto.DeliveryDate.HasValue && dto.DeliveryDate.Value != default)
                    vehicle.DeliveryDate = dto.DeliveryDate.Value;

                if (!string.IsNullOrWhiteSpace(dto.CurrentLocation))
                    vehicle.CurrentLocation = dto.CurrentLocation;

                if (dto.CostPrice.HasValue && dto.CostPrice.Value >= 0)
                    vehicle.CostPrice = dto.CostPrice.Value;

                if (dto.DealerReceivedDate.HasValue && dto.DealerReceivedDate.Value != default)
                    vehicle.DealerReceivedDate = dto.DealerReceivedDate.Value;

                if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                    vehicle.ImageUrl = dto.ImageUrl;

                // Không set: DealerId, ColorId, VersionId
                _unitOfWork.ElectricVehicleRepository.Update(vehicle);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Vehicle updated successfully.",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }


        }

        public async Task<ResponseDTO> UpdateVehicleStatusAsync(Guid vehicleId, StatusVehicle newStatus)
        {
            try
            {
                var vehicle = await _unitOfWork.ElectricVehicleRepository.GetByIdsAsync(vehicleId);
                if (vehicle == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Vehicle not found",
                        StatusCode = 404,
                    };
                }

                vehicle.Status = newStatus;
                _unitOfWork.ElectricVehicleRepository.Update(vehicle);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Vehicle status updated successfully",
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }
    }
}
