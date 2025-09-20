using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Vehicle;
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
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateVehicleAsync(CreateVehicleDTO vehicleCreateDTO)
        {
            try
            {
                var IsExistVehicle = await _unitOfWork.VehicleRepository.IsExistByName(vehicleCreateDTO.Name);
                if (IsExistVehicle = true)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Vehicle already exists"
                    };
                }

                Vehicle vehicle = new Vehicle
                {
                    Name = vehicleCreateDTO.Name,
                    Model = vehicleCreateDTO.Model,
                    Version = vehicleCreateDTO.Version,
                    BasePrice = vehicleCreateDTO.BasePrice,
                };

                await _unitOfWork.VehicleRepository.AddAsync(vehicle);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Vehicle created successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> DeleteVehicleAsync(Guid id)
        {
            try
            {
                var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(id);
                if (vehicle is null)
                {    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Vehicle not found"
                    };
                }

                vehicle.IsApprovedByManager = false;
                _unitOfWork.VehicleRepository.Update(vehicle);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Vehicle deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetAllVehicles(
            string? filterOn,
            string? filterQuery,
            string? sortBy,
            bool? isAscending
            )
        {
            try
            {
                var vehicles = (await _unitOfWork.VehicleRepository.GetAllAsync()).
                    Where(v => v.IsApprovedByManager == true);
                if (!string.IsNullOrEmpty(filterOn) && !string.IsNullOrEmpty(filterQuery))
                {
                    string filter = filterOn.ToLower();
                    string query = filterQuery.ToUpper();

                    vehicles = filter switch
                    {
                        "name" => vehicles.Where(v => v.Name.ToUpper().Contains(query)),
                        "model" => vehicles.Where(v => v.Model.ToUpper().Contains(query)),
                        "version" => vehicles.Where(v => v.Version.ToUpper().Contains(query)),
                        _ => vehicles
                    };
                }
                if (!string.IsNullOrEmpty(sortBy))
                {
                    vehicles = sortBy.Trim().ToLower() switch
                    {
                        "name" => isAscending == true ? vehicles.OrderBy(v => v.Name) : vehicles.OrderByDescending(v => v.Name),
                        "model" => isAscending == true ? vehicles.OrderBy(v => v.Model) : vehicles.OrderByDescending(v => v.Model),
                        "version" => isAscending == true ? vehicles.OrderBy(v => v.Version) : vehicles.OrderByDescending(v => v.Version),
                        _ => vehicles
                    };
                }
                var getVehicles = _mapper.Map<List<GetVehicleDTO>>(vehicles);

                if (vehicles is null || !vehicles.Any())
                {   return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "No vehicles found"
                    };
                }
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Vehicles retrieved successfully",
                    Result = getVehicles
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleByIdAsync(Guid id)
        {
            try
            {
                var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(id);
                if (vehicle is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Vehicle not found"
                    };
                }
                
                var getVehicle = _mapper.Map<GetVehicleDTO>(vehicle);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Vehicle retrieved successfully",
                    Result = getVehicle
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleByName(string name)
        {
            try
            {
                var vehicle = _unitOfWork.VehicleRepository.GetByNameAsync(name);
                if( vehicle is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Vehicle not found"
                    };
                }

                var getVehicle = _mapper.Map<GetVehicleDTO>(vehicle);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Vehicle retrieved successfully",
                    Result = getVehicle
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> UpdateVehicleAsync(Guid vehicleid, UpdateVehicleDTO vehicleUpdateDTO)
        {
            try
            {
                if (vehicleid == Guid.Empty)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Vehicle ID is required"
                    };
                }

                var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(vehicleid);
                if (vehicle is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Vehicle not found"
                    };
                }

                if (!string.IsNullOrEmpty(vehicleUpdateDTO.Name))
                {
                    var isExistVehicle = await _unitOfWork.VehicleRepository.IsExistByNameAndId(vehicleUpdateDTO.Name, vehicleid);
                    if (isExistVehicle)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Vehicle name already exists"
                        };
                    }

                    vehicle.Name = vehicleUpdateDTO.Name;
                }

                if (!string.IsNullOrEmpty(vehicleUpdateDTO.Model))
                {
                    vehicle.Model = vehicleUpdateDTO.Model;
                }
                if (!string.IsNullOrEmpty(vehicleUpdateDTO.Version))
                {
                    vehicle.Version = vehicleUpdateDTO.Version;
                }
                if (vehicleUpdateDTO.BasePrice > 0)
                {
                    vehicle.BasePrice = vehicleUpdateDTO.BasePrice;
                }

                _unitOfWork.VehicleRepository.Update(vehicle);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Vehicle updated successfully",
                    Result = _mapper.Map<GetVehicleDTO>(vehicle)
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }
    }
}
