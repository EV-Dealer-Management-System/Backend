using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
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

        public Task<ResponseDTO> GetAllVehiclesAsync()
        {
            throw new NotImplementedException();
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

        public async Task<ResponseDTO> UpdateVehicleAsync(Guid vehicleId, UpdateElectricVehicleDTO updateElectricVehicleDTO)
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

                vehicle.VIN = updateElectricVehicleDTO.VIN;
                vehicle.Status = updateElectricVehicleDTO.Status;
                vehicle.ManufactureDate = updateElectricVehicleDTO.ManufactureDate;
                vehicle.ImportDate = updateElectricVehicleDTO.ImportDate;
                vehicle.WarrantyExpiryDate = updateElectricVehicleDTO.WarrantyExpiryDate;
                vehicle.CurrentLocation = updateElectricVehicleDTO.CurrentLocation;
                vehicle.CostPrice = updateElectricVehicleDTO.CostPrice;
                vehicle.ImageUrl = updateElectricVehicleDTO.ImageUrl;

                if (vehicle is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Vehicle is null.",
                        StatusCode = 400
                    };
                }

                _unitOfWork.ElectricVehicleRepository.Update(vehicle);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Update vehicle successfully.",
                    StatusCode = 200
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
    }
}
