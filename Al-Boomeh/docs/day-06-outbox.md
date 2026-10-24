### Incident log :
i add the line `Environment.Exit(1)` after the `await transaction.CommitAsync();` and the order was saved in the database with Id `300046` but in the log
no notifications was found its mean because the test line "sever crached" between to oprations
- The order. It is Pending, and its stock was taken.
- The RabbitMQ dashboard. No message arrived. Both queues stayed empty.
- The tally. The store's sales tally did not move.

**nothing in the system said that something went wrong. No error, no failed job, no alert.**

## the delay in orderplace :
03:11:07.9744714 − 03:11:06.8974400 = 1.0770314 s ≈ 1077 ms
this the time between order saved in the database and snet the notification 
Background Service was used with polling 500ms

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

And the rule being applied is: the messageId must be stored in the database so that the message can be tracked and we can know whether it was sent or not.

## Two controlls break the rule of the day :

- POST /api/Orders/send-messages calls SendConfirmation
- POST /api/Orders/place-test-order calls BasicPublishAsync
 **they are both are test tools. They save nothing in SQL, so they open no gap**

 ## Why a BackgroundService :
 I used a background service because the requirement is that the delay be under two seconds, so I can control how often it polls for new messages and set it to every 500 ms.

If Hangfire were used instead, the shortest interval it can run at is one minute, so it isn't suitable here.

