using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System.ComponentModel;
using System.Xml.Linq;

namespace SWP391Web.Application.Validations
{
    public class RegisterCustomerValidation : AbstractValidator<RegisterCustomerDTO>
    {
        public RegisterCustomerValidation()
        {
            RuleFor(rc => rc.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(rc => rc.Password)
                .NotEmpty().WithMessage("Password is required")
                .Matches(@"^(?=.*[A-Z])(?=.*[!@#$%^&*(),.?""{}|<>])(?=.*\d).{8,}$")
                .WithMessage("Password must contain at least 1 special character, 1 uppercase latter, 1 number and at least 8 characters");

            RuleFor(rc => rc.ConfirmPassword)
                .Equal(rc => rc.Password).WithMessage("Password do not match");

            RuleFor(rc => rc.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .Matches(@"^[\p{L}\s]+$").WithMessage("Full name can only contain latter and space");
        }
    }
}
