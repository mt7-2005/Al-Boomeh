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

## Part 3 :

### Part 0:
- Al-Boomeh/BackgroundServices/OutBoxRelay.cs 
**what change :** add the OrderStatusChanged publisher 
- Al-Boomeh/Program.cs
**what change :** Register the consumer and the interface for event
- Al-BoomehServices/Consumers/NotificationsOrderPlaced.cs
**what change :** send for the hub client for store 
- Al-BoomehServices/Dtos/OrderDtos.cs
**what change :** add the OrderChangedDto
- Al-BoomehServices/Publishers/OrderPlaced.cs
**what change :** make the type for the message take it from the messageDto
- Al-BoomehServices/Services/OrderService.cs
**what change :** save the OutBoxMessage in the UpdateOrderStatus method

### Part 1:

Status: Delivered
6:23:05 PM — connected
6:23:05 PM — watching order 300056
6:23:25 PM — order 300056 is now Accepted
6:24:20 PM — order 300056 is now Preparing
6:24:27 PM — order 300056 is now OutForDelivery
6:24:36 PM — order 300056 is now Delivered

### Part 2:
The Day 5 fanout exchange for “order placed” let me add this reaction without touching the existing flow: I simply bound a new queue and consumer to it, which publishes through the SignalR hub to notify the partners’ clients whenever an order is placed. Since fanout delivers a copy of every message to each bound queue, the original consumers kept working unchanged. For order status updates I created a separate exchange, so those events are routed independently from the order-placed ones.

## Part 4 :

### Part 0:
- Bug One : the customer miss the updates for the order status 
- Bug two : after reconnected nothing send , but when the customer click in Watch order it return the current status for the order its meane if he dont click it evry thing look fine 

### Part 1:
Status: Preparing
8:38:37 PM — connected
8:38:37 PM — watching order 300056
8:38:37 PM — loaded status Pending
8:39:08 PM — connection lost, trying again every 5 seconds
8:40:45 PM — reconnected
8:40:45 PM — loaded status Preparing
 
### Part 2:
If we load the status first and the Partner moves the order forward before the connection joins the group, the update is lost: the broadcast goes only to the members the group has at the time it is sent, so this client never receives it. The page keeps showing the old status, and the customer never learns that the order’s status has changed.

## Part 5 :

### Part 0:
- Before fix : the copy 5101 is the copy that handled the moves and the moves that reached on the page is {Accepted,Preparing}

### Part 1:
- backplane : is the problem when the SingleR connection know onlly the his own copy and dont see the others

### Part 2:
- the consumer needs one queue for each copy because evry SingleR connection onlly the his own copy , and the day 5  work queue dont need that because the exchager in his publisher is direct not a fanout 

### Part 3:
| **queue name** | `order-status-changed.<GUID>` | Unique per copy, so no two copies share a queue. Each copy therefore gets its own copy of every message. |
| **durable** | `false` | The queue is not kept across a broker restart. It is tied to the copy's life, so there is nothing to save. |
| **exclusive** | `true` | Only the connection that declared the queue can use it, and RabbitMQ deletes it when that connection closes. This is what makes the queue disappear when the copy stops. |
| **autoDelete** | `true` | The queue is deleted after its last consumer unsubscribes. Extra cleanup in case the exclusive rule does not fire. |

### Part 4:
**Name**: Sticky sessions
**How it works:** The load balancer remembers which server first handled a client, usually by setting a cookie or hashing the client’s IP. It then sends every later request from that client to the same server. SignalR needs this because a connection’s state lives in the memory of the server that created it, so the negotiate request and the connection that follows must reach the same server.


## Part 6 :

### Part 0:
- **before :** 9,960
- **after :** 53,700
- **the difference :** 219

### Part 1:
- **Did Swagger still answer while the 200 connections were open?** Yes

### Part 2:
- The server finds out a connection is dead through small ping messages (empty messages that just mean “I’m still alive”). The server and the client each send a ping every 15 seconds. If the server receives no message at all from the client for 30 seconds, it treats the connection as dead, closes it, and frees its memory. So when a laptop lid is closed and the browser can’t say goodbye, the server notices after about 30 seconds of silence.

## Part 7 :

### Part 0:
- Accepted -> Preparing -> OutForDelivery -> Delivered

### Part 1:
- Yes, my system saves the message with its body, which is an OrderStatusChangedDto, and it also saves the order status history, so what happens to an order can be tracked.
 ### Part 2:
- A socket cannot store events that happen while the connection is down. SignalR does not queue messages for a disconnected client, so anything sent in that time is lost for that connection.

## The hardest thing today:
- The hardes thing is Part 5 — Two copies of the app, one customer
- It was a new problem, and I got to know new settings in the RabbitMQ consumer.