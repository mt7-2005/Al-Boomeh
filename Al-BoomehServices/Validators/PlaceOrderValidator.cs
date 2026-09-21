using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class PlaceOrderValidator : AbstractValidator<PlaceOrderDTO>
    {
        public PlaceOrderValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum();

            RuleFor(x => x.PaymentMethod)
                .IsInEnum();

            RuleFor(x => x.CustomerId)
                .GreaterThan(0);

            RuleFor(x => x.AddressId)
                .GreaterThan(0)
                .When(o => o.Type == enOrderType.Delivery);

            RuleFor(x => x.Tips)
                .GreaterThanOrEqualTo(0)
                .When(o => o.Type == enOrderType.Delivery);


            RuleFor(x => x.StoreNotes)
                .MaximumLength(500)
                .When(x => x.StoreNotes != null);

            RuleFor(x => x.DriverNotes)
                .MaximumLength(500)
                .When(x => x.DriverNotes != null);

            RuleFor(x => x.DriverInstructions)
                .MaximumLength(500)
                .When(x => x.DriverInstructions != null);

            RuleFor(x => x.VoucherCode)
                .MaximumLength(50)
                .When(x => x.VoucherCode != null);

            RuleFor(x => x.Latitude)
                .Must(AppValidators.BeAValidCoordinate)
                .When(x => x.Latitude != null)
                .When(o => o.Type == enOrderType.Delivery);

            RuleFor(x => x.Longitude)
                .Must(AppValidators.BeAValidCoordinate)
                .When(x => x.Longitude != null)
                .When(o => o.Type == enOrderType.Delivery);
        }
    }

}
