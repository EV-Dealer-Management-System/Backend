using System.ComponentModel.DataAnnotations;

namespace SWP391Web.Application.DTO.Auth
{
    public class RegisterUserDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
        public string FullName { get; set; } = default!;
    }
}
