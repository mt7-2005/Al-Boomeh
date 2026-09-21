using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class VoucherValidator : AbstractValidator<VoucherDTO>
    {
        public VoucherValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.ExpirationDate)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow);

            RuleFor(x => x.MinimumDiscount)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.MaximumDiscount)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x)
                .Must(x => x.MaximumDiscount >= x.MinimumDiscount)
                .OverridePropertyName(nameof(VoucherDTO.MaximumDiscount));
        }
    }
}
