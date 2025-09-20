using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Validations
{
    public class UpdateVehicleAsyncValidation : AbstractValidator<UpdateVehicleAsyncValidation>
    {
        public UpdateVehicleAsyncValidation()
        {
            RuleFor(x => x).NotEmpty().WithMessage("VehicleId is Required");
        }
    }
}
