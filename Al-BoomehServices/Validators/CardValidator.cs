using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class CardValidator : AbstractValidator<CardDTO>
    {
        public CardValidator()
        {
            RuleFor(x => x.CardNum)
                .NotEmpty()
                .CreditCard();

            RuleFor(x => x.CVC)
                .InclusiveBetween(100, 9999);

            RuleFor(x => x.ExpirationDate)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow);
        }
    }
}
