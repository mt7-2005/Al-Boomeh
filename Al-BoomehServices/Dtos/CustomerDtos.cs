namespace Al_BoomehDAL.Classes
{
    public class CreateCustomerDTO
    {
        public string? ImagePath { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public enGender Gender { get; set; }
    }

    public class UpdateCustomerDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class ResponseCustomerDTO
    {

        public int id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public enGender Gender { get; set; }
        public string? ImagePath { get; set; }
        public enCustomerStatus Status { get; set; }
       

    }

    public class AddressDTO
    {
        public int id { get; set; }
        public int customerId { get; set; }
        public string AddressName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? BuildNum { get; set; }
        public string? StreetName { get; set; }
        public string? AdditionalPhone { get; set; }
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public int? FloorNum { get; set; }
        public int? HomeNum { get; set; }

    }

    public class CardDTO
    {
        public string CardNum { get; set; } = string.Empty;
        public int CVC { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public class IssueDTO
    {
        public int Id { get; set; }
        public string Name {  get; set; }= string.Empty;
        public int CustomerId { get; set; }
        public int IssueId { get; set; }
    }

    public class FavStoresDTO
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        public int CustomerId { get; set; }
    }

    public class VoucherDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal MaximumDiscount { get; set; }
        public decimal MinimumDiscount { get; set; }
    }
}
