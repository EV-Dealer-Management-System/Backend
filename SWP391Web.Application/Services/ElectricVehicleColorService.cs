using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicleColor;
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
    public class ElectricVehicleColorService : IElectricVehicleColorService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public ElectricVehicleColorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateColorAsync(CreateElectricVehicleColorDTO createElectricVehicleColorDTO)
        {
            try
            {
                var isColorCodeExist = await _unitOfWork.ElectricVehicleColorRepository
                    .IsColorExistsByCode(createElectricVehicleColorDTO.ColorCode);
                if (isColorCodeExist)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color code already exists.",
                        StatusCode = 404
                    };
                }
                var isColorNameExist = await _unitOfWork.ElectricVehicleColorRepository.IsColorExistsByName(createElectricVehicleColorDTO.ColorName);
                if (isColorNameExist)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color name already exists.",
                        StatusCode = 404
                    };
                }

                ElectricVehicleColor electricVehicleColor = new ElectricVehicleColor
                {
                    ColorCode = createElectricVehicleColorDTO.ColorCode,
                    ColorName = createElectricVehicleColorDTO.ColorName,
                    ExtraCost = createElectricVehicleColorDTO.ExtraCost
                };

                await _unitOfWork.ElectricVehicleColorRepository.AddAsync(electricVehicleColor, CancellationToken.None);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color created successfully.",
                    StatusCode = 201
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

        public Task<ResponseDTO> DeleteColorAsync(Guid colorId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDTO> GetAllColorsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDTO> GetColorByCodeAsync(string colorCode)
        {
            try
            {
                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByCodeAsync(colorCode);
                if (color is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color not found.",
                        StatusCode = 404
                    };
                }
                var getColor = _mapper.Map<GetElectricVehicleColorDTO>(color);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color retrieved successfully.",
                    Result = getColor,
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

        public async Task<ResponseDTO> GetColorByIdAsync(Guid colorId)
        {
            try
            {
                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByIdsAsync(colorId);
                if(color is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color not found.",
                        StatusCode = 404
                    };
                }

                var getColor = _mapper.Map<GetElectricVehicleColorDTO>(color);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color retrieved successfully.",
                    Result = getColor,
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

        public async Task<ResponseDTO> GetColorByNameAsync(string colorName)
        {
            try
            {
                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByNameAsync(colorName);
                if (color is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color not found.",
                        StatusCode = 404
                    };
                }
                var getColor = _mapper.Map<GetElectricVehicleColorDTO>(color);
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color retrieved successfully.",
                    Result = getColor,
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

        public async Task<ResponseDTO> UpdateColorAsync(Guid colorId, UpdateElectricVehicleColor updateElectricVehicleColor)
        {
            try
            {
                var color = await _unitOfWork.ElectricVehicleColorRepository.GetByIdsAsync(colorId);

                if (color is null)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Color not found.",
                        StatusCode = 404
                    };
                }

                color.ExtraCost = (decimal)updateElectricVehicleColor.ExtraCost;
                 _unitOfWork.ElectricVehicleColorRepository.Update(color);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO()
                {
                    IsSuccess = true,
                    Message = "Color updated successfully.",
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
    }
}
