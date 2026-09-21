using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class UpdateExtraValidator : AbstractValidator<UpdateExtraDTO>
    {
        public UpdateExtraValidator()
        {
            RuleFor(x => x.ExtraName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue);
        }
    }
}
