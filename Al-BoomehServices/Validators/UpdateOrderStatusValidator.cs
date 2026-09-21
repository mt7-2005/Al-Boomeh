using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDTO>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum();

            RuleFor(x => x.StoreId)
                .GreaterThan(0);
        }
    }
}
