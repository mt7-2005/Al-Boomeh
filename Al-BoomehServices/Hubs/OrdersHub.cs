using Al_BoomehDAL.Interfaces;
using Al_BoomehDAL.Models;
using Al_BoomehServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Al_Boomeh.Hubs
{

    [Authorize]
    public class OrdersHub : Hub
    {
        public readonly ICurrentUser _user;
        public readonly IOrderService _orderService;
        public readonly IUserService _userService;
        public OrdersHub(ICurrentUser user, IOrderService orderService, IUserService userService)
        {
            _user = user;
            _orderService = orderService;
            _userService = userService;
        }
        public async Task WatchOrder(long orderId)
        {

            if (!_user.IsAuthenticated) throw new HubException("You may not watch this order.");

            if (_user.Role == User.UserRole.Customer || _user.Role == User.UserRole.Admin)
            {
                var customerId = await _orderService.GetOrderOwnerId((int)orderId);

                var user = await _userService.GetUser((Guid)_user.UserId!);

                if (user == null) throw new HubException("No user found");

                if (user.CustomerId != customerId) throw new HubException("You may not watch this order.");

                await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");

            }
            else
            {
                throw new HubException("You may not watch this order.");
            }

        }

        public async Task WatchStore(long storeId)
        {
            if (!_user.IsAuthenticated) throw new HubException("You may not watch this store.");

            if (_user.Role == User.UserRole.Partner || _user.Role == User.UserRole.Admin)
            {
                if (_user.StoreId != storeId) throw new HubException("You may not watch this store.");

                await Groups.AddToGroupAsync(Context.ConnectionId, $"store-{storeId}");

            }
            else
            {
                throw new HubException("You may not watch this store.");
            }
        }
    }
}
