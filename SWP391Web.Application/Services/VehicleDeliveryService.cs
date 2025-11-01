using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.VehicleDelivery;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class VehicleDeliveryService : IVehicleDeliveryService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;

        public VehicleDeliveryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> CreateVehicleDeliveryAsync(CreateVehicleDeliveryDTO createVehicleDeliveryDTO)
        {
        
            var bookingEV = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(createVehicleDeliveryDTO.BookingEVId);
            if (bookingEV == null)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Booking not found",
                    StatusCode = 400
                };
            }

            VehicleDelivery vehicleDelivery = new VehicleDelivery
            {
                BookingEVId = bookingEV.Id,
                Description = createVehicleDeliveryDTO.Description,
                CreatedDate = DateTime.UtcNow,
                Status = DeliveryStatus.Preparing,
                UpdateAt = DateTime.UtcNow,
            };

            await _unitOfWork.VehicleDeliveryRepository.AddAsync(vehicleDelivery, CancellationToken.None);

            foreach (var dt in bookingEV.BookingEVDetails)
            {
                var bookedVehicles = await _unitOfWork.ElectricVehicleRepository
                    .GetBookedVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                foreach (var ev in bookedVehicles.Take(dt.Quantity))
                {
                    ev.Status = ElectricVehicleStatus.InTransit;
                    _unitOfWork.ElectricVehicleRepository.Update(ev);
                }
            }
            await _unitOfWork.SaveAsync();
            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Create Vehicle Delivery successfully",
                StatusCode = 200
            };
        }

        public async Task<ResponseDTO> GetAllVehicleDelivery(DeliveryStatus? status = null)
        {
            try
            {
                Func<IQueryable<VehicleDelivery>, IQueryable<VehicleDelivery>> includes = q =>
                q.Include(vd => vd.BookingEV)
                    .ThenInclude(b => b.Dealer);

                Expression<Func<VehicleDelivery, bool>>? filter = null;
                if (status.HasValue)
                {
                    filter = vd => vd.Status == status.Value;
                }

                var deliveries = await _unitOfWork.VehicleDeliveryRepository.GetAllAsync(
                    filter: filter,
                    includes: includes);

                var getDeliveries = _mapper.Map<List<VehicleDelivery>>(deliveries);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all deliveries successfully",
                    StatusCode = 200,
                    Result = getDeliveries
                };
                   
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public Task<ResponseDTO> GetVehicleDeliveryById(Guid deliveryId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDTO> UpdateVehicleDeliveryStatus(ClaimsPrincipal user, Guid deliveryId, DeliveryStatus newStatus)
        {
            throw new NotImplementedException();
        }
    }
}
