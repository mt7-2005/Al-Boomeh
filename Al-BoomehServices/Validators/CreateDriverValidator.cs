using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class CreateDriverValidator : AbstractValidator<CreateDriverDTO>
    {
        public CreateDriverValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Phone)
                .NotEmpty()
                .Matches(@"^\+?[0-9]{10,15}$");

            RuleFor(x => x.Gender)
                .IsInEnum();

            RuleFor(x => x.VehicleType)
                .IsInEnum();
        }
    }
}
