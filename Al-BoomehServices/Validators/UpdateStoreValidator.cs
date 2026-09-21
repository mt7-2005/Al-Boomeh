using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class UpdateStoreValidator : AbstractValidator<UpdateStoreDTO>
    {
        public UpdateStoreValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Area)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Latitude)
                .NotEmpty()
                .Must(AppValidators.BeAValidCoordinate);

            RuleFor(x => x.Longitude)
                .NotEmpty()
                .Must(AppValidators.BeAValidCoordinate);

            RuleFor(x => x.Tax)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Tax.HasValue);
        }
    }
}
