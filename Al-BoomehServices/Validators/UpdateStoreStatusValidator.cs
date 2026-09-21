using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class UpdateStoreStatusValidator : AbstractValidator<UpdateStoreStatusDTO>
    {
        public UpdateStoreStatusValidator()
        {
            RuleFor(x => x.StoreStatus)
                .IsInEnum();
        }
    }
}
