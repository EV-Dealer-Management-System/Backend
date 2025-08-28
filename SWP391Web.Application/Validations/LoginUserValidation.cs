using FluentValidation;
using SWP391Web.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Validations
{
    public class LoginUserValidation : AbstractValidator<LoginUserDTO>
    {
        public LoginUserValidation() 
        {
            RuleFor(l => l.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(l => l.Password).NotEmpty().WithMessage("Password is required");
        }
    }
}
