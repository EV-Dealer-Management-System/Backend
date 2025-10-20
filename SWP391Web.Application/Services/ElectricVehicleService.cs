using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class ElectricVehicleService : IElectricVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IS3Service _s3Service;
        public ElectricVehicleService(IUnitOfWork unitOfWork, IMapper mapper, IS3Service s3Service)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
        }

        public async Task<ResponseDTO> CreateVehicleAsync(CreateElecticVehicleDTO createElectricVehicleDTO)
        {
            try
            {
                var warehouse = await _unitOfWork.WarehouseRepository
                    .GetWarehouseByIdAsync(createElectricVehicleDTO.WarehouseId);
                if (warehouse is null || warehouse.WarehouseType != WarehouseType.EVInventory
                    || !warehouse.EVCInventory.IsActive)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Warehouse not found or not an EV Inventory warehouse.",
                        StatusCode = 404
                    };
                }

                var vinError = new List<string>();
                var vinCreated = new List<string>();

                foreach (var vin in createElectricVehicleDTO.VINList)
                {
                    var isVinExist = await _unitOfWork.ElectricVehicleRepository.IsVehicleExistsByVIN(vin);
                    if (isVinExist)
                    {
                        vinError.Add(vin);
                        continue;
                    }

                    ElectricVehicle electricVehicle = new ElectricVehicle
                    {
                        ElectricVehicleTemplateId = createElectricVehicleDTO.ElectricVehicleTemplateId,
                        WarehouseId = createElectricVehicleDTO.WarehouseId,
                        VIN = vin,
                        Status = StatusVehicle.Available,
                        ManufactureDate = createElectricVehicleDTO.ManufactureDate,
                        ImportDate = createElectricVehicleDTO.ImportDate,
                        WarrantyExpiryDate = createElectricVehicleDTO.WarrantyExpiryDate,
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
                    vinCreated.Add(vin);
                }

                if (!vinCreated.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "All vin are duplicated ",
                        StatusCode = 400,
                        Result = new { VINError = vinError }
                        
                    };
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = $"Created {vinCreated.Count} vehicles successfully. {vinError.Count} VIN(s) duplicated.",
                    StatusCode = 201,
                    Result = new
                    {
                        CreatedVIN = vinCreated,
                        VINError = vinError
                    }
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

        public async Task<ResponseDTO> GetAllVehiclesAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 400
                    };
                }
                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                List<ElectricVehicle> vehicles;
                if (role == StaticUserRole.Admin || role == StaticUserRole.EVMStaff)
                {
                    vehicles = (await _unitOfWork.ElectricVehicleRepository.GetAllAsync(
                        includes: q => q
                        .Include(ev => ev.ElectricVehicleTemplate)
                            .ThenInclude(evt => evt.Version)
                                .ThenInclude(v => v.Model)
                        .Include(ev => ev.Warehouse))).ToList();
                }
                else if (role == StaticUserRole.DealerManager || role == StaticUserRole.DealerStaff)
                {
                    var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, CancellationToken.None);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found.",
                            StatusCode = 404
                        };
                    }
                    vehicles = await _unitOfWork.ElectricVehicleRepository.GetAllVehicleWithDetailAsync();
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No permission",
                        StatusCode = 404
                    };
                }

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

        public async Task<ResponseDTO> GetAvailableQuantityByModelVersionColorAsync(Guid modelId, Guid versionId, Guid colorId)
        {
            try
            {


                var quantity = await _unitOfWork.ElectricVehicleRepository
                    .GetAvailableQuantityByModelVersionColorAsync(modelId, versionId, colorId);

                if (quantity == 0)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "No available vehicles for the selected model, version, and color.",
                        StatusCode = 400
                    };
                }

                return new ResponseDTO()
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Available quantity retrieved successfully",
                    Result = quantity
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

        public async Task<ResponseDTO> GetDealerInventoryAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var vehicles = new List<ElectricVehicle>();

                if (role == StaticUserRole.Admin || role == StaticUserRole.EVMStaff)
                {
                    vehicles = await _unitOfWork.ElectricVehicleRepository.GetAllVehicleWithDetailAsync();
                }
                else
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404
                        };
                    }

                    vehicles = await _unitOfWork.ElectricVehicleRepository.GetDealerInventoryAsync(dealer.Id);
                }
                if (vehicles == null)
                {
                    return new ResponseDTO
                    {
                         IsSuccess = false,
                         Message = "No vehicle in inventory",
                         StatusCode = 404
                    };
                }
                

                var getDealerInventory = vehicles.GroupBy(ev => new
                {
                    ev.ElectricVehicleTemplate.Version.Model.ModelName,
                    ev.ElectricVehicleTemplate.Version.VersionName,
                    ev.ElectricVehicleTemplate.Color.ColorName
                })
                    .Select(g => new
                    {
                        ModelName = g.Key.ModelName,
                        VersionName = g.Key.VersionName,
                        ColorName = g.Key.ColorName,
                        Quantity = g.Count()
                    })
                    .OrderBy(x => x.ModelName)
                    .ThenBy(x => x.VersionName)
                    .ThenBy(x => x.ColorName)
                    .ToList();

                return new ResponseDTO
                {

                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get Dealer Inventory successfully",
                    Result = getDealerInventory
                };

                    
            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetSampleVehiclesAsync(ClaimsPrincipal user)
        {
            //try
            //{
            //    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //    if(userId == null)
            //    {
            //        return new ResponseDTO
            //        {
            //            IsSuccess = false,
            //            Message = "User not found",
            //            StatusCode = 400
            //        };
            //    }

            //    var role = user.FindFirst(ClaimTypes.Role)?.Value;

            //    List<ElectricVehicle> vehicles;
            //    if(role == StaticUserRole.Admin || role == StaticUserRole.EVMStaff)
            //    {
            //        vehicles = (await _unitOfWork.ElectricVehicleRepository.GetAllAsync()).ToList();
            //    }
            //    else
            //    {
            //        var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
            //        if(dealer == null)
            //        {
            //            return new ResponseDTO
            //            {
            //                IsSuccess = false,
            //                Message = " Dealer not found",
            //                StatusCode = 404
            //            };
            //        }

            //        vehicles = await _unitOfWork.ElectricVehicleRepository.GetAllVehicleWithDetailAsync();
            //    }

            //    //Group into 1
            //    var groupVehicles = vehicles
            //        .GroupBy(v => new {v.ElectricVehicleTemplate.VersionId, v.ElectricVehicleTemplate.ColorId })
            //        .Select(g => g.FirstOrDefault(v => _unitOfWork.EVAttachmentRepository.GetAttachmentsByElectricVehicleId(v.Id).Any())
            //        ??g.First())
            //        .ToList();

            //    var getVehicles = _mapper.Map<List<GetElecticVehicleDTO>>(groupVehicles);

            //    foreach (var vehicle in getVehicles)
            //    {
            //        var attachments = _unitOfWork.EVAttachmentRepository
            //            .GetAttachmentsByElectricVehicleId(vehicle.Id);

            //        var urlList = new List<string>();
            //        foreach (var att in attachments)
            //        {
            //            var url = _s3Service.GenerateDownloadUrl(att.Key);
            //            urlList.Add(url);
            //        }

            //        vehicle.ImgUrl = urlList;
            //    }

            //    return new ResponseDTO
            //    {
            //        IsSuccess = true,
            //        Message = "Get sample vehicles successfully.",
            //        StatusCode = 200,
            //        Result = getVehicles
            //    };

            //}
            //catch (Exception ex)
            //{
            //    return new ResponseDTO
            //    {
            //        IsSuccess = false,
            //        Message = ex.Message,
            //        StatusCode = 500
            //    };
            //}
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

                if (dto.DealerReceivedDate.HasValue && dto.DealerReceivedDate.Value != default)
                    vehicle.DealerReceivedDate = dto.DealerReceivedDate.Value;

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

        public async Task<ResponseDTO> UpdateVehicleStatusAsync(Guid vehicleId, ElectricVehicleStatus newStatus)
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
