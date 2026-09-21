using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class OrderFeedbackValidator : AbstractValidator<OrderFeedbackDTO>
    {
        public OrderFeedbackValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0);

            RuleFor(x => x.CustomerId)
                .GreaterThan(0);

            RuleFor(x => x.Rate)
                .IsInEnum();

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => x.Notes != null);
        }
    }

}
