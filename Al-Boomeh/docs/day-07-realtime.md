## Part 0 :

### Part 0:

- the request method :GET ws://localhost:5155/ws/echo
- the Upgrade header : websocket
- Status code : 101 Switching Protocols

### Part 1:
- 101 Switching Protocols is the status code. It means the server accepted the request to switch the connection from HTTP to the WebSocket protocol.

### Part 2:
| Data   | Length | Time         | Direction         |
|--------|--------|--------------|-------------------|
| hello  | 5      | 01:56:22.407 | ⬆️        |
| hello  | 5      | 01:56:22.415 | ⬇️        |
| second | 6      | 01:56:31.592 | ⬆️        |
| second | 6      | 01:56:31.597 | ⬇️        |
| third  | 5      | 01:56:35.448 | ⬆️        |
| third  | 5      | 01:56:35.449 | ⬇️        |

### Part 3:
- the customer count =20,001
- number of request = 20,001
- the count of seconds in one hour is 3600s
- 3600 / 5 = 720 this is the number of how many poling request for 1 customer
- 720 * 20,001= 14,400,720 that the number of how many requsts would server get

## Part 1 :

### Part 0:
- Request name : `https://localhost:7027/hubs/orders/negotiate?access_token=&negotiateVersion=1`
### Part 1:
- Method : POST
### Part 2:
- Status : 404 Not Found
### Part 3:
- What SignalR sends : Before opening a WebSocket, SignalR sends an HTTP POST negotiate request to the server, which carries the access token and the negotiateVersion parameter.
### Part 4:
- The message : Status: unknown
5:21:39 PM — error: Failed to complete negotiation with the server: Error: : Status code '404' Either this is not a SignalR endpoint or there is a proxy blocking the connection.

## Part 2 :
### Part 0: Risk of a token in a query string

**Risk:** A token in the query string can leak. URLs are written to server, proxy and browser logs, saved in browser history, and can show up in the `Referer` header. Anyone who gets the URL can reuse the token for other purposes.

**What makes it smaller:** The token is only used on the `/hubs` endpoint (the WebSocket/SignalR connection), so its exposure is limited. Short-lived tokens or HTTPS would reduce it further.

---

### Part 1: `WatchOrder` method

```csharp
public async Task WatchOrder(long orderId)
{
    if (!_user.IsAuthenticated)
        throw new HubException("You may not watch this order.");

    if (_user.Role == User.UserRole.Customer || _user.Role == User.UserRole.Admin)
    {
        var customerId = await _orderService.GetOrderOwnerId((int)orderId);

        var user = await _userService.GetUser((Guid)_user.UserId!);
        if (user == null) throw new Exception("no user found");

        if (user.CustomerId != customerId)
            throw new HubException("You may not watch this order.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }
    else
    {
    throw new HubException("You may not watch this order.");
    }
}
```

---

### Part 2: The attack

**Before the change.** The hub allowed watching any order, including orders that belong to other customers and orders that don't exist:

```
5:37:32 PM — connected
5:37:32 PM — watching order 2
5:37:36 PM — watching order 23
5:37:41 PM — watching order 87878
5:37:45 PM — watching order 8787898
```

**After the change.** The hub refuses orders that aren't mine. Order 300056 is my own order, so it is allowed:

```
5:39:02 PM — connected
5:39:02 PM — error: An unexpected error occurred invoking 'WatchOrder' on the server. HubException: You may not watch this order.
5:39:07 PM — watching order 300056
5:39:12 PM — error: An unexpected error occurred invoking 'WatchOrder' on the server. HubException: You may not watch this order.
5:39:14 PM — error: An unexpected error occurred invoking 'WatchOrder' on the server. HubException: You may not watch this order.
```

---

### Part 3: Partner test

Partner test lines: their own store is allowed, and the other store is refused.

```
5:43:50 PM — connected
5:43:50 PM — error: An unexpected error occurred invoking 'WatchStore' on the server. HubException: You may not watch this order.
5:43:53 PM — watching store 42
5:43:56 PM — error: An unexpected error occurred invoking 'WatchStore' on the server. HubException: You may not watch this order.
```

- **Own store:** `watching store 42` is allowed.
- **Other stores:** the other two attempts are refused.