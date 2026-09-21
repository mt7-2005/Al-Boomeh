using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Al_BoomehServices.Validators.AppValidators;

namespace Al_BoomehServices.Validators
{
    public class CreateOrderCartValidator : AbstractValidator<CreateOrderCartDTO>
    {
        public CreateOrderCartValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0);

            RuleFor(x => x.StoreId)
                .GreaterThan(0);

            RuleFor(x => x.OrderLines)
                .NotEmpty();

            RuleForEach(x => x.OrderLines)
                .SetValidator(new CreateOrderLineValidator());
        }
    }

}
