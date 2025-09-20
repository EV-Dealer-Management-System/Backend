using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.VehicleColorDTO;
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
    public class VehicleColorService : IVehicleColorService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public VehicleColorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateVehicleColorAsync(CreateVehicleColorDTO vehicleColorCreateDTO)
        {
            try
            {
                var isExistedColor = await _unitOfWork.VehicleColorRepository.IsExistByColorName(vehicleColorCreateDTO.ColorName);
                if(isExistedColor is true)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Color is existed",
                    };
                }
                
                VehicleColor vehicleColor = new VehicleColor
                {                     
                   ColorName = vehicleColorCreateDTO.ColorName,
                   ExtraPrice = vehicleColorCreateDTO.ExtraPrice,
                };

                 await _unitOfWork.VehicleColorRepository.AddAsync(vehicleColor);
                    await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Create color successfully",
                    Result = vehicleColor
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO> DeleteVehicleColorAsync(Guid id)
        {
            try
            {
                var VehicleColor =  await _unitOfWork.VehicleColorRepository.GetByIdAsync(id);
                if (VehicleColor is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Color not found",
                    };
                _unitOfWork.VehicleColorRepository.Update(VehicleColor);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Delete color successfully",
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

        public Task<ResponseDTO> GetAllVehicleColors()
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDTO> GetVehicleColorByIdAsync(Guid id)
        {
            try
            {
                var vehicleColor = await _unitOfWork.VehicleColorRepository.GetByIdAsync(id);
                if (vehicleColor is null)
                                    {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Color not found",
                    };
                }

                var getVehicleColorDTO = _mapper.Map<GetVehicleColorDTO>(vehicleColor);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get color successfully",
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleColorByName(string name)
        {
            try
            {
                var vehicleColor = await _unitOfWork.VehicleColorRepository.GetByNameAsync(name);
                if (vehicleColor is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Color not found",
                    };
                }

                var getVehicleColorDTO = _mapper.Map<GetVehicleColorDTO>(vehicleColor);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get color successfully",
                    Result = getVehicleColorDTO
                };
            }
            catch
            (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,
                };
            }
        }

    }
}
