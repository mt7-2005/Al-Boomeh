using static Al_BoomehDAL.Models.User;

namespace Al_BoomehServices.Services
{
    public class UserDto
    {
        public int? CustomerId { get; set; }
        public UserRole Role { get; set; }
        public int? StoreId { get; set; }
    }

    public class PartnerDto
    {
        public int StoreId { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
