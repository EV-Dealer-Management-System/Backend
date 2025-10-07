using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.BookingEV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IBookingEVService
    {
        Task <ResponseDTO> CreateBookingEVAsync(CreateBookingEVDTO createBookingEVDTO);
    }
}
