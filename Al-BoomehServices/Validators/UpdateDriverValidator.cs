using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class UpdateDriverValidator : AbstractValidator<UpdateDriverDTO>
    {
        public UpdateDriverValidator()
        {
            RuleFor(x => x.DriverId)
                .GreaterThan(0)
                .When(x => x.DriverId.HasValue);
        }
    }
}
