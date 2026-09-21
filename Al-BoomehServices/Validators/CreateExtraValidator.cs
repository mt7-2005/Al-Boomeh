using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class CreateExtraValidator : AbstractValidator<CreateExtraDTO>
    {
        public CreateExtraValidator()
        {
            RuleFor(x => x.ExtraName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.ProductId)
                .GreaterThan(0);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue);
        }
    }
}
