using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class AddressValidator : AbstractValidator<AddressDTO>
    {
        public AddressValidator()
        {
            RuleFor(x => x.customerId)
                .GreaterThan(0);

            RuleFor(x => x.AddressName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Phone)
                .NotEmpty()
                .Matches(@"^\+?[0-9]{10,15}$");

            RuleFor(x => x.AdditionalPhone)
                .Matches(@"^\+?[0-9]{10,15}$")
                .When(x => !string.IsNullOrEmpty(x.AdditionalPhone));

            RuleFor(x => x.Latitude)
                .NotEmpty()
                .Must(AppValidators.BeAValidCoordinate);

            RuleFor(x => x.Longitude)
                .NotEmpty()
                .Must(AppValidators.BeAValidCoordinate);

            RuleFor(x => x.BuildNum)
                .GreaterThan(0)
                .When(x => x.BuildNum.HasValue);

            RuleFor(x => x.FloorNum)
                .GreaterThanOrEqualTo(0)
                .When(x => x.FloorNum.HasValue);

            RuleFor(x => x.HomeNum)
                .GreaterThan(0)
                .When(x => x.HomeNum.HasValue);

            RuleFor(x => x.StreetName)
                .MaximumLength(150)
                .When(x => x.StreetName != null);

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => x.Notes != null);
        }
    }
}
