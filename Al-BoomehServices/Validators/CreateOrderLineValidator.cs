using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class CreateOrderLineValidator : AbstractValidator<CreateOrderLineDTO>
    {
        public CreateOrderLineValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0);

            RuleFor(x => x.ProductId)
                .GreaterThan(0);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);

           

            RuleFor(x => x.Notes)
                .MaximumLength(300)
                .When(x => x.Notes != null);
        }
    }
}
