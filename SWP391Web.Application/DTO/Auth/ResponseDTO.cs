using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.Auth
{
    public class ResponseDTO
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public object? Result { get; set; }
        public int StatusCode { get; set; } = 200;
    }
}
