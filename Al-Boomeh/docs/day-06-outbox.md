### Incident log :
i add the line `Environment.Exit(1)` after the `await transaction.CommitAsync();` and the order was saved in the database with Id `300046` but in the log
no notifications was found its mean because the test line "sever crached" between to oprations
- The order. It is Pending, and its stock was taken.
- The RabbitMQ dashboard. No message arrived. Both queues stayed empty.
- The tally. The store's sales tally did not move.

**nothing in the system said that something went wrong. No error, no failed job, no alert.**

## the delay in orderplace :
The relay was waiting 5000 ms between rounds, which broke the 2-second limit.
I set it back to 500 ms and measured again with new orders after the fix:

| Outbox row | CreatedAtUtc | SentAtUtc | Delay |
|---|---|---|---|
| 85 | 2026-11-02 21:05:04.844 | 2026-11-02 21:05:05.374 | 530 ms |
| 86 | 2026-11-02 21:46:13.251 | 2026-11-02 21:46:13.470 | 218 ms |
| 87 | 2026-11-02 21:46:29.234 | 2026-11-02 21:46:29.555 | 321 ms |
| 88 | 2026-11-02 21:46:58.387 | 2026-11-02 21:46:58.623 | 237 ms |

Result: every delay is under 2 seconds. The slowest was 530 ms. The relay waits at most 500 ms between rounds, plus the publish time. ✅

## Test A :
The app ran and did stop (crashed). When I restarted it, the relay read 50 rows from the Outbox and published the events that update the analytics and send the notifications.

After the restart, the analytics tally for the store went up by the order's amount: it was 1546.45 before and 1553.75 after (order total 7.30). ✅
## Test B :
The message cannot be sent before the order is saved, because the outbox row must be saved in the same transaction as the order. Therefore, if the relay loop finds no row, no message is sent.

## Test C — the relay dies halfway
Indeed, when I put an exception before updating the message's status and setting it to sent=true, the message was retried, but what stopped it was what was done in day-05 with the unique messageId, and it was skipped: "[15:53:44 INF] Message 3d6bc00e-d487-4828-8259-9d1340d87176 was already processed by notifications. Skipping it."

## Test D — move the message id and watch everything break
Yes, indeed, the notification was sent again:

[16:15:44 INF] Analytics updated for store with Id 1
info: 10/21/2026 16:15:44.716 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
      Executed DbCommand (23ms) [Parameters=[@__storeId_0='1' (Nullable = true)], CommandType='Text', CommandTimeout='30']
      SELECT [u].[Email]
      FROM [Users] AS [u]
      WHERE [u].[IsDeleted] = CAST(0 AS bit) AND [u].[Role] = 1 AND [u].[StoreID] = @__storeId_0 AND [u].[Email] IS NOT NULL
[16:15:44 INF] Notification to legros, gutmann and lindgren@al-boomeh.com: New order Order 300064, total 7.80
[16:15:48 INF] Analytics updated for store with Id 1
info: 10/21/2026 16:15:48.298 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
      Executed DbCommand (78ms) [Parameters=[@__orderId_0='300064'], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [t].[Email]
      FROM [Order] AS [o]
      INNER JOIN (
          SELECT [c].[Id], [c].[Email]
          FROM [Customer] AS [c]
          WHERE [c].[IsDeleted] = CAST(0 AS bit)
      ) AS [t] ON [o].[CustomerID] = [t].[Id]
      WHERE [o].[IsDeleted] = CAST(0 AS bit) AND [o].[Id] = @__orderId_0
[16:15:48 INF] Notification to romaine28@gmail.com: Your order was placed Order 300064, total 7.80
info: 10/21/2026 16:15:48.329 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
      Executed DbCommand (26ms) [Parameters=[@__storeId_0='1' (Nullable = true)], CommandType='Text', CommandTimeout='30']
      SELECT [u].[Email]
      FROM [Users] AS [u]
      WHERE [u].[IsDeleted] = CAST(0 AS bit) AND [u].[Role] = 1 AND [u].[StoreID] = @__storeId_0 AND [u].[Email] IS NOT NULL
[16:15:48 INF] Notification to legros, gutmann and lindgren@al-boomeh.com: New order Order 300064, total 7.80

The rule: make the message id once, save it with the outbox row, and send the same id every time.

`SentAtUtc` is what tells me if a message was sent. The message id has another job: it lets the consumer know if it has seen this message before.

If a new id is made on each send, every copy looks like a new message, so the consumer can never skip a duplicate. With one stored id, the copy has the same id, and the consumer skips it.
## Two controlls break the rule of the day :

- POST /api/Orders/send-messages calls SendConfirmation
- POST /api/Orders/place-test-order calls BasicPublishAsync
 **they are both are test tools. They save nothing in SQL, so they open no gap**

 ## Why a BackgroundService :
 I used a background service because the requirement is that the delay be under two seconds, so I can control how often it polls for new messages and set it to every 500 ms.

If Hangfire were used instead, the shortest interval it can run at is one minute, so it isn't suitable here.

## Part 4 :
## Part 4 :
**Experiment 1 (no transaction).** I turned off the transaction and saved the tally first. I threw once before the "handled" row was saved. When the message came again, the order was counted twice (the tally went up by 2).

**Experiment 2 (my real code).** I put the transaction back and threw once after both saves, before `CommitAsync`. When the message came again, the tally went up only once.

Inside one transaction, the order of the two writes does not matter.

## Two copies of the app, one outbox :

- In the first test with 10 orders, I found no duplicate messages. The distribution was 5 per worker, and the RabbitMQ queue went from 10 to 5 to 0.

- Then I ran a bigger test with 100 orders to try to trigger the error, and again I found no duplicate messages. Still, it will certainly happen under more favorable conditions and heavier load.

- Yes, the order did break, and messages are now sent in a disorganized way. For example:

[12:40:25 INF] Analytics updated for store with Id 1
[12:40:25 INF] Notification to romaine28@gmail.com: Your order was placed Order 300051, total 7.80
[12:40:25 INF] Analytics updated for store with Id 1
[12:40:25 INF] Notification to legros, gutmann and lindgren@al-boomeh.com: New order Order 300051, total 7.80
[12:40:25 INF] Analytics updated for store with Id 1
[12:40:25 INF] Analytics updated for store with Id 1
[12:40:25 INF] Consume at 2026-11-05T09:40:25.4232551Z

The analytics was updated 3 times against a single notification to the store. Also, the confirmation is not sent until all the commands have finished for all the messages.

