using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class UpdateOrderTimeValidator : AbstractValidator<UpdateOrderTimeDTO>
    {
        public UpdateOrderTimeValidator()
        {
            RuleFor(x => x.EstimatedPreparingTime)
                .GreaterThan(0);

            RuleFor(x => x.EstimatedDeliveryTime)
                .GreaterThan(0);
        }
    }
}
