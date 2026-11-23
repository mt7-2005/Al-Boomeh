## Part 0

### 1. The three things in the Headers tab
- Request method: `GET` (`ws://localhost:5155/ws/echo`)
- Request header: `Upgrade: websocket`
- Status code: `101 Switching Protocols`

### 2. What the 101 means
`101 Switching Protocols` means the server accepted the request to switch this connection from normal HTTP to the WebSocket protocol. After the 101, the same connection stays open, and both the browser and the server can send messages on it at any time.

### 3. The Messages tab
The Messages tab showed 6 rows: 3 sent by the browser (arrow up) and 3 received (arrow down). Each message came back once.

| Data   | Length | Time         | Direction |
|--------|--------|--------------|-----------|
| hello  | 5      | 01:56:22.407 | ⬆️ up     |
| hello  | 5      | 01:56:22.415 | ⬇️ down   |
| second | 6      | 01:56:31.592 | ⬆️ up     |
| second | 6      | 01:56:31.597 | ⬇️ down   |
| third  | 5      | 01:56:35.448 | ⬆️ up     |
| third  | 5      | 01:56:35.449 | ⬇️ down   |

### 4. Polling cost
The SQL query:

```sql
SELECT COUNT(*) FROM Customers;
```

The result is 20,001 customers.

- One hour has 3,600 seconds.
- One customer polling every 5 seconds: 3,600 / 5 = 720 requests in one hour.
- All customers: 720 × 20,001 = 14,400,720 requests in one hour.

---

## Part 1

Run on the `http` profile (`http://localhost:5155/tracker.html`), with the hub not yet built.

### 1. The line the page wrote
```
Status: unknown
5:21:39 PM — error: Failed to complete negotiation with the server: Error: : Status code '404' Either this is not a SignalR endpoint or there is a proxy blocking the connection.
```

### 2. The request that failed
- Name: `http://localhost:5155/hubs/orders/negotiate?access_token=&negotiateVersion=1`
- Method: `POST`
- Status: `404 Not Found`

### 3. What SignalR sends before it opens a WebSocket
Before it opens a WebSocket, SignalR sends a normal HTTP `POST` request to `/hubs/orders/negotiate`, to agree on the connection and the transport.

---

## Part 2

### 1. The risk of a token in the query string
**Risk:** a URL is written to server logs, proxy logs and browser history, and it can appear in the `Referer` header. Anyone who can read those logs can copy the token and use it until it expires. A header is not logged like this.

**What makes it smaller:** a short-lived token, so a leaked one stops working soon. Also, do not log the query string of `/hubs` requests. We only read the token from the query string for paths that start with `/hubs`, so no other endpoint is exposed this way. HTTPS protects the URL on the network, but it does not keep the URL out of the logs.

### 2. `WatchOrder`
```csharp
public async Task WatchOrder(long orderId)
{

    if (!_user.IsAuthenticated) throw new HubException("You may not watch this order.");

    if (_user.Role == User.UserRole.Customer || _user.Role == User.UserRole.Admin)
    {
        var customerId = await _orderService.GetOrderOwnerId((int)orderId);

        if (customerId == null) throw new HubException("You may not watch this order.");

        var user = await _userService.GetUser((Guid)_user.UserId!);

        if (user == null) throw new HubException("You may not watch this order.");

        if (user.CustomerId != customerId) throw new HubException("You may not watch this order.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");

    }
    else
    {
        throw new HubException("You may not watch this order.");
    }

}

```

### 3. The attack
**Before the change.** The hub allowed watching any order, including orders of other customers and orders that do not exist:

```
5:37:32 PM — connected
5:37:32 PM — watching order 2
5:37:36 PM — watching order 23
5:37:41 PM — watching order 87878
5:37:45 PM — watching order 8787898
```

**After the change.** The hub refuses orders that are not mine. Order 300056 is my own order, so it is allowed:

```
5:39:02 PM — connected
5:39:02 PM — error: An unexpected error occurred invoking 'WatchOrder' on the server. HubException: You may not watch this order.
5:39:07 PM — watching order 300056
5:39:12 PM — error: An unexpected error occurred invoking 'WatchOrder' on the server. HubException: You may not watch this order.
5:39:14 PM — error: An unexpected error occurred invoking 'WatchOrder' on the server. HubException: You may not watch this order.
```

### 4. The Partner test
Partner test lines: their own store is allowed, and the other store is refused.

```
5:43:50 PM — connected
5:43:50 PM — error: An unexpected error occurred invoking 'WatchStore' on the server. HubException: You may not watch this store.
5:43:53 PM — watching store 42
5:43:56 PM — error: An unexpected error occurred invoking 'WatchStore' on the server. HubException: You may not watch this store.
```

- **Own store:** `watching store 42` is allowed.
- **Other stores:** the other two attempts are refused.

---

## Part 3

### 1. Files I changed
- `Al-BoomehServices/Services/OrderService.cs` — `UpdateOrderStatus` writes an `OrderStatusChanged` row to the outbox in the same transaction as the status change. `UpdateOrderDriver` and `DeclineAbandonedOrders` do the same, through one helper method.
- `Al-Boomeh/BackgroundServices/OutBoxRelay.cs` — sends `OrderStatusChanged` rows with the new publisher.
- `Al-BoomehServices/Publishers/OrderStatusChanged.cs` and `Interfaces/IOrderStatusChanged.cs` — new publisher for the `order-status-changed` fanout exchange.
- `Al-BoomehServices/Publishers/OrderPlaced.cs` — takes the message type from the `MessageDto`.
- `Al-BoomehServices/Dtos/OrderDtos.cs` — new `OrderStatusChangedDto`.
- `Al-BoomehServices/Hubs/OrdersHub.cs` — new hub with `WatchOrder` and `WatchStore`, both with owner checks.
- `Al-BoomehServices/Hubs/OrderPushConsumer.cs` — new consumer. It pushes `OrderStatusChanged` to the order's group and `OrderPlaced` to the store's group. Each copy of the app has its own queue for each event.
- `Al-BoomehServices/Consumers/NotificationsOrderPlaced.cs` — the `OrderPlaced` push was removed from here.
- `Al-Boomeh/Controllers/OrdersController.cs` — new `GET /api/orders/{id}/status`.
- `Al-Boomeh/Program.cs` — `AddSignalR`, `MapHub`, `UseStaticFiles`, `UseWebSockets`, `/ws/echo`, the token from the query string for `/hubs`, and `AddHostedService<OrderPushConsumer>`.
- `Al-Boomeh/wwwroot/tracker.html` — the tracker page.
- `README.md` — how to open the tracker and where to get a token.

### 2. The consumer log lines for the four moves
```
Status: Delivered
6:23:05 PM — connected
6:23:05 PM — watching order 300056
6:23:25 PM — order 300056 is now Accepted
6:24:20 PM — order 300056 is now Preparing
6:24:27 PM — order 300056 is now OutForDelivery
6:24:36 PM — order 300056 is now Delivered
```

### 3. How the fanout exchange let me add this reaction
A fanout exchange gives a copy of every message to every queue bound to it. So adding the push was only a new consumer with its own queue, bound to the exchange. The publisher, the outbox, `PlaceOrder`, the notifications consumer and the analytics consumer did not change. The same is true for `OrderPlaced`: the new queue is bound to the existing `order-placed` exchange, and the old consumers keep getting their own copy.

---

## Part 4

### 1. The two bugs
**Bug one: the missed updates are gone.** I cut the connection (Offline plus a restart) and moved the order two steps. After the page reconnected, it still showed the old status. It happened because a WebSocket keeps nothing. The consumer sent the two updates to the order's group while nobody was in it, so they went nowhere, and they are not stored anywhere.

**Bug two: after a reconnect, the page is in no group.** The page said `reconnected`, and everything looked fine, but the next move did not appear. It happened because a reconnect makes a new connection with a new connection id. A group holds connection ids, and the new id was in no group. The restart also emptied all groups, because they live in the memory of the app. The page looks healthy and gets nothing, which is worse than bug one.

### 2. The second test, with the fixed page
```
Status: Preparing
8:38:37 PM — connected
8:38:37 PM — watching order 300056
8:38:37 PM — loaded status Pending
8:39:08 PM — connection lost, trying again every 5 seconds
8:40:45 PM — reconnected
8:40:45 PM — loaded status Preparing
8:40:48 PM — order 300056 is now OutForDelivery
```

### 3. Loading the status first, joining the group second
If the page loads the status first, an update can be lost between two moments: the moment the API answers with the status, and the moment `WatchOrder` finishes adding the connection to the group. If the Partner moves the order in that gap, the push goes to a group that does not contain this connection, so it never arrives. The page keeps the status it loaded before the move, and nothing corrects it. If the page joins the group first, a move after the join arrives by the socket, and a move before the join is already in the status that the API returns.

---

## Part 5

### 1. Before the fix
Copy 5101 handled the moves, and the moves that reached the page were `Accepted` and `Preparing`.

### 2. The backplane problem
SignalR knows only the connections of its own copy. The page is connected to copy 5101. When copy 5102 handles an event, it sends it to its own connections, and the customer is not one of them, so the message goes nowhere and nothing fails. A backplane passes every message to every copy, so the copy that holds the connection can send it.

### 3. Why one queue for each copy here, and one shared queue for the Day 5 work queue
A work queue is one queue that many workers share. RabbitMQ gives each message to one worker only. We want that when a job must happen once, like sending an email. If two workers both did it, the customer would get two emails.

Here we want the opposite. The connection of a customer lives in the memory of one copy, and we do not know which copy. So every copy needs every message, and each copy checks whether it holds the connection. This needs one queue for each copy, all bound to the fanout exchange, so each queue gets its own copy of every message. If the copies shared one queue, each event would reach one copy only, and the customer would miss the updates that went to the other copy. This is the bug I saw before the fix.

The difference is not the exchange type. It is the question: "should this message be handled once in total, or once in every copy?"

### 4. The queue settings
| Setting | Value | What it does |
|---|---|---|
| queue name | `order-status-changed.<GUID>` | A new name in every copy, so no two copies share a queue. Each copy gets every message |
| `durable` | `false` | The queue is not kept when the broker restarts. It lives as long as the copy, so there is nothing to save |
| `exclusive` | `true` | Only the connection that declared the queue can use it, and RabbitMQ deletes it when that connection closes |
| `autoDelete` | `true` | The queue is deleted when its last consumer stops. Extra cleanup |

The `OrderPlaced` queue uses the same four settings.

### 5. The SignalR feature that solves this
**Name:** Backplane. The common choices are Redis (`Microsoft.AspNetCore.SignalR.StackExchangeRedis`) and Azure SignalR Service.

**How it works:** every copy publishes the messages it sends to the backplane, and every copy reads from it. Then each copy sends the message to its own connections. So a message that one copy sends reaches the connections of all copies.

**What it would have done here:** the push consumer could keep one shared queue, like a Day 5 work queue. Whichever copy got the event would send it through the backplane, and the page on the other copy would still get it. I would not have needed one queue for each copy.

(Sticky sessions are a different feature: they keep one client on one server. They do not pass messages between copies.)

---

## Part 6

### 1. Memory before and after
- Before: 9,960 KB
- After: 53,700 KB
- Difference: 53,700 − 9,960 = 43,740 KB
- Cost of one connection: 43,740 KB / 200 ≈ 219 KB

### 2. Did Swagger still answer?
Yes. A normal endpoint in Swagger answered as usual while the 200 connections were open.

### 3. How the server finds a dead connection
SignalR sends a small ping message to check that the other side is still alive. The server and the client each send a ping every 15 seconds. If the server gets no message at all from the client for 30 seconds, it treats the connection as dead, closes it and frees its memory. So when a laptop closes its lid and the browser cannot say goodbye, the server learns about it after about 30 seconds of silence.

---

## Part 7

### 1. The updates that reached nobody
Four updates, in this order:

1. `Accepted`
2. `Preparing`
3. `OutForDelivery` (your order is on the way)
4. `Delivered`

### 2. How I would find out
Nothing in my system tells me today. The consumer sends to the group, and `SendAsync` to an empty group gives no error, no warning and no number. The outbox row and the order status history show that the status changed, but they say nothing about whether anybody got the push. To find out, I would have to count the connections in the group before the send, and log a warning or a metric when the count is zero.

### 3. What a socket can never do
A socket can never reach a customer whose app is closed, because it needs an open and running app on both ends.

---

## The hardest thing today
The hardest part was Part 5, two copies of the app and one customer. Before, I thought one queue was always the right choice, because the Day 5 work queue was one queue. I only understood the problem when I ran two copies: the log of 5102 showed it handled a move, and nothing appeared on the page. That showed me that each copy needs its own copy of every message. I solved it by reading the RabbitMQ documentation about queues, and I learned the settings `exclusive` and `autoDelete`. They make a queue that belongs to one copy and disappears when the copy stops.