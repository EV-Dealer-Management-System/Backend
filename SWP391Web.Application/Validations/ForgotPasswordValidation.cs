using FluentValidation;
using SWP391Web.Application.DTO.Auth;

namespace SWP391Web.Application.Validations
{
    public class ForgotPasswordValidation : AbstractValidator<ForgotPasswordDTO>
    {
        public ForgotPasswordValidation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
