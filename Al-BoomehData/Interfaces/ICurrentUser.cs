using Al_BoomehDAL.Models;

namespace Al_BoomehDAL.Interfaces
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        User.UserRole Role { get; }
        int? StoreId { get; }
        bool IsAuthenticated { get; }
    }
}