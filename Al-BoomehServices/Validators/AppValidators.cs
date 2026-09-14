using Al_BoomehDAL.Classes;
using Al_BoomehServices.Services;
using FluentValidation;
using System;

namespace Al_BoomehServices.Validators
{
    public class AppValidators
    {
        // ---------- Customers ----------

        public class CreateCustomerValidator : AbstractValidator<CreateCustomerDTO>
        {
            public CreateCustomerValidator()
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .EmailAddress();

                RuleFor(x => x.Phone)
                    .NotEmpty()
                    .Matches(@"^\+?[0-9]{10,15}$");

                RuleFor(x => x.DateOfBirth)
                    .NotEmpty()
                    .LessThan(DateTime.UtcNow);
                    

                RuleFor(x => x.Gender)
                    .IsInEnum();

                RuleFor(x => x.ImagePath)
                    .MaximumLength(500)
                    .When(x => x.ImagePath != null);
            }

           
        }

        public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDTO>
        {
            public UpdateCustomerValidator()
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .EmailAddress();
            }
        }

        public class AddressValidator : AbstractValidator<AddressDTO>
        {
            public AddressValidator()
            {
                RuleFor(x => x.customerId)
                    .GreaterThan(0);

                RuleFor(x => x.AddressName)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.Phone)
                    .NotEmpty()
                    .Matches(@"^\+?[0-9]{10,15}$");

                RuleFor(x => x.AdditionalPhone)
                    .Matches(@"^\+?[0-9]{10,15}$")
                    .When(x => !string.IsNullOrEmpty(x.AdditionalPhone));

                RuleFor(x => x.Latitude)
                    .NotEmpty()
                    .Must(BeAValidCoordinate);

                RuleFor(x => x.Longitude)
                    .NotEmpty()
                    .Must(BeAValidCoordinate);

                RuleFor(x => x.BuildNum)
                    .GreaterThan(0)
                    .When(x => x.BuildNum.HasValue);

                RuleFor(x => x.FloorNum)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.FloorNum.HasValue);

                RuleFor(x => x.HomeNum)
                    .GreaterThan(0)
                    .When(x => x.HomeNum.HasValue);

                RuleFor(x => x.StreetName)
                    .MaximumLength(150)
                    .When(x => x.StreetName != null);

                RuleFor(x => x.Notes)
                    .MaximumLength(500)
                    .When(x => x.Notes != null);
            }
        }

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

        public class IssueValidator : AbstractValidator<IssueDTO>
        {
            public IssueValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(200);

                RuleFor(x => x.CustomerId)
                    .GreaterThan(0);

                RuleFor(x => x.IssueId)
                    .GreaterThan(0);
            }
        }

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

        public class VoucherValidator : AbstractValidator<VoucherDTO>
        {
            public VoucherValidator()
            {
                RuleFor(x => x.CustomerId)
                    .GreaterThan(0);

                RuleFor(x => x.Amount)
                    .GreaterThan(0);

                RuleFor(x => x.Code)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.ExpirationDate)
                    .NotEmpty()
                    .GreaterThan(DateTime.UtcNow);

                RuleFor(x => x.MinimumDiscount)
                    .GreaterThanOrEqualTo(0);

                RuleFor(x => x.MaximumDiscount)
                    .GreaterThanOrEqualTo(0);

                RuleFor(x => x)
                    .Must(x => x.MaximumDiscount >= x.MinimumDiscount)
                    .OverridePropertyName(nameof(VoucherDTO.MaximumDiscount));
            }
        }

        // ---------- Categories ----------

        public class CreateCategoryValidator : AbstractValidator<CreateCategoryDTO>
        {
            public CreateCategoryValidator()
            {
                RuleFor(x => x.CategoryName)
                    .NotEmpty()
                    .MaximumLength(50);
            }
        }

        public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDTO>
        {
            public UpdateCategoryValidator()
            {
                RuleFor(x => x.CategoryName)
                    .NotEmpty()
                    .MaximumLength(50);
            }
        }

        // ---------- Drivers ----------

        public class CreateDriverValidator : AbstractValidator<CreateDriverDTO>
        {
            public CreateDriverValidator()
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.Phone)
                    .NotEmpty()
                    .Matches(@"^\+?[0-9]{10,15}$");

                RuleFor(x => x.Gender)
                    .IsInEnum();

                RuleFor(x => x.VehicleType)
                    .IsInEnum();
            }
        }

        public class UpdateDriverInfoValidator : AbstractValidator<UpdateDriverInfoDTO>
        {
            public UpdateDriverInfoValidator()
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.Phone)
                    .NotEmpty()
                    .Matches(@"^\+?[0-9]{10,15}$");

                RuleFor(x => x.VehicleType)
                    .IsInEnum();
            }
        }

        public class DriverIssueValidator : AbstractValidator<DriverIssueDTO>
        {
            public DriverIssueValidator()
            {
                RuleFor(x => x.DriverId)
                    .GreaterThan(0);

                RuleFor(x => x.IssueId)
                    .GreaterThan(0);
            }
        }

        // ---------- Extras ----------

        public class CreateExtraValidator : AbstractValidator<CreateExtraDTO>
        {
            public CreateExtraValidator()
            {
                RuleFor(x => x.ExtraName)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(x => x.ProductId)
                    .GreaterThan(0);

                RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.Price.HasValue);
            }
        }

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

        // ---------- Orders ----------

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

                RuleFor(x => x.ExtraId)
                    .GreaterThan(0)
                    .When(x => x.ExtraId.HasValue);

                RuleFor(x => x.Notes)
                    .MaximumLength(300)
                    .When(x => x.Notes != null);
            }
        }

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
                    .GreaterThan(0);

                RuleFor(x => x.Tips)
                    .GreaterThanOrEqualTo(0);

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
                    .Must(BeAValidCoordinate)
                    .When(x => x.Latitude != null);

                RuleFor(x => x.Longitude)
                    .Must(BeAValidCoordinate)
                    .When(x => x.Longitude != null);
            }
        }

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

        public class UpdateDriverValidator : AbstractValidator<UpdateDriverDTO>
        {
            public UpdateDriverValidator()
            {
                RuleFor(x => x.DriverId)
                    .GreaterThan(0)
                    .When(x => x.DriverId.HasValue);
            }
        }

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

        // ---------- Products ----------

        public class CreateProductValidator : AbstractValidator<CreateProductDTO>
        {
            public CreateProductValidator()
            {
                RuleFor(x => x.ProductName)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.StockQuantity)
                    .GreaterThanOrEqualTo(0);

                RuleFor(x => x.ProductPrice)
                    .GreaterThan(0);

                RuleFor(x => x.ProductDescription)
                    .NotEmpty()
                    .MaximumLength(1000);

                RuleFor(x => x.StoreId)
                    .GreaterThan(0);

                RuleFor(x => x.CategoryId)
                    .GreaterThan(0);

                RuleFor(x => x.ImagePath)
                    .MaximumLength(500)
                    .When(x => x.ImagePath != null);
            }
        }

        public class UpdateProductValidator : AbstractValidator<UpdateProductDTO>
        {
            public UpdateProductValidator()
            {
                RuleFor(x => x.ProductName)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.StockQuantity)
                    .GreaterThanOrEqualTo(0);

                RuleFor(x => x.ProductPrice)
                    .GreaterThan(0);

                RuleFor(x => x.ProductDescription)
                    .NotEmpty()
                    .MaximumLength(1000);

                RuleFor(x => x.CategoryId)
                    .GreaterThan(0);

                RuleFor(x => x.ImagePath)
                    .MaximumLength(500)
                    .When(x => x.ImagePath != null);
            }
        }

        public class UpdateProductStockValidator : AbstractValidator<UpdateProductStockDTO>
        {
            public UpdateProductStockValidator()
            {
                RuleFor(x => x.StoreId)
                    .GreaterThan(0);

                RuleFor(x => x.StockQuantity)
                    .GreaterThanOrEqualTo(0);
            }
        }

        // ---------- Stores ----------

        public class CreateStoreValidator : AbstractValidator<CreateStoreDTO>
        {
            public CreateStoreValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.Area)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.Latitude)
                    .NotEmpty()
                    .Must(BeAValidCoordinate);

                RuleFor(x => x.Longitude)
                    .NotEmpty()
                    .Must(BeAValidCoordinate);

                RuleFor(x => x.Tax)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.Tax.HasValue);

                RuleFor(x => x.StoreStatus)
                    .IsInEnum();
            }
        }

        public class UpdateStoreValidator : AbstractValidator<UpdateStoreDTO>
        {
            public UpdateStoreValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.Area)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.Latitude)
                    .NotEmpty()
                    .Must(BeAValidCoordinate);

                RuleFor(x => x.Longitude)
                    .NotEmpty()
                    .Must(BeAValidCoordinate);

                RuleFor(x => x.Tax)
                    .GreaterThanOrEqualTo(0)
                    .When(x => x.Tax.HasValue);
            }
        }

        public class UpdateStoreStatusValidator : AbstractValidator<UpdateStoreStatusDTO>
        {
            public UpdateStoreStatusValidator()
            {
                RuleFor(x => x.StoreStatus)
                    .IsInEnum();
            }
        }

        public class StoreIssueValidator : AbstractValidator<StoreIssueDTO>
        {
            public StoreIssueValidator()
            {
                RuleFor(x => x.StoreId)
                    .GreaterThan(0);

                RuleFor(x => x.IssueId)
                    .GreaterThan(0);
            }
        }

        // ---------- Users ----------

        public class PartnerValidator : AbstractValidator<PartnerDto>
        {
            public PartnerValidator()
            {
                RuleFor(x => x.StoreId)
                    .GreaterThan(0);

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .EmailAddress();

                RuleFor(x => x.Password)
                    .NotEmpty()
                    .MinimumLength(8);
            }
        }

        // ---------- shared helper ----------

        private static bool BeAValidCoordinate(string? value)
        {
            return double.TryParse(value, out _);
        }
    }
}