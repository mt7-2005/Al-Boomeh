using Al_BoomehDAL.Models;
using Al_BoomehDAL.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Al_Boomeh.Services
{
    public class CurrentUserService : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid UserId
        {
            get
            {
                var claim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
            }
        }

        public User.UserRole Role
        {
            get
            {
                var roleClaim = User?.FindFirstValue(ClaimTypes.Role);
                return Enum.TryParse<User.UserRole>(roleClaim, out var role)
                    ? role
                    : Al_BoomehDAL.Models.User.UserRole.None; 
            }
        }
        public int? StoreId
        {
            get
            {
                var claim = User?.FindFirstValue("storeId");
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }
}