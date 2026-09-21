using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class FavStoresValidator : AbstractValidator<FavStoresDTO>
    {
        public FavStoresValidator()
        {
            RuleFor(x => x.StoreId)
                .GreaterThan(0);

            RuleFor(x => x.CustomerId)
                .GreaterThan(0);
        }
    }

}
