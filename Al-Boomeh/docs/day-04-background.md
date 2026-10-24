# Hangfire Review — Day Summary

## 1. Fire-and-Forget Jobs

A **Fire-and-Forget Job** is executed once after being explicitly enqueued.

### Example: Send SMS OTP

The OTP job is suitable for Hangfire because:

- It is triggered by an event, such as requesting an OTP.
- It only needs to run once for that request.
- The job should survive an application restart.
- Hangfire provides persistence and retry behavior if the job fails.

---

## 2. Recurring Jobs

A **Recurring Job** executes according to a defined schedule using a Cron expression.

### Examples

#### Cancel Abandoned Orders

- Runs according to a defined schedule.
- Finds orders that have remained pending for too long.
- Must survive application restarts.
- Should be retryable if an execution fails.
- The operation should be safe to repeat.

#### Cleanup Expired Refresh Tokens

- Runs on a daily schedule.
- Deletes expired or old revoked refresh tokens.
- Must survive application restarts.
- Should be retryable if the database operation fails.
- The stock remained unchanged after the crash and retry: Product A stayed at 12 and Product B at 2. Both orders' histories changed from `Pending` to `Decline` after Fix 8.
#### Create Daily Reports

- Runs once per day.
- Generates reports for stores.
- Must survive application restarts.
- Should be retryable if report generation fails.
- The operation should be repeat-safe to avoid creating duplicate reports.
- "order count" means in the daily report. It counts Delivered, Cancelled and Declined orders, while the revenue counts Delivered orders only, Revenue: Only orders with a Delivered status are included, as cancelled and rejected orders are excluded from revenue and profit.


---

## 3. Continuation Jobs

A **Continuation Job** starts after another Hangfire job has completed.

The implementation uses:

```csharp
BackgroundJob.Enqueue(...)
```

followed by:

```csharp
BackgroundJob.ContinueJobWith(...)
```

This allows the second job to depend on the completion of the first job.

### Example

The daily report job creates the reports first.

After the report job completes, the notification job is executed to send the reports to the Partners.

The important deciding factor is that the second operation depends on the completion of the first one.

---

## 4. BackgroundService vs Hangfire

The main difference is the **lifetime and persistence requirements** of the work.

### Hangfire

Use Hangfire when the job:

- Must survive application restarts.
- Needs persistence.
- Needs automatic retries.
- Is a one-off or scheduled task.
- Should not depend on the application continuously running.

### BackgroundService

Use `BackgroundService` when the work:

- Is a continuous loop.
- Needs to process events continuously.
- Is tied to the lifetime of the application.
- Does not require Hangfire's persistent job storage.

For example, a continuously running worker that drains an in-memory queue is better implemented as a `BackgroundService`.

---

# 5. When Should Hangfire Be Used?

The deciding factor is not simply whether something runs every 30 minutes.

The important questions are:

- Does the work need to survive an application restart?
- Does it need retries?
- Is it a one-off job?
- Is it scheduled?
- Is it a continuous loop?
- Does the work depend on persistent storage?

## 5.1 Send SMS OTP

**Tool:** Hangfire Fire-and-Forget

**Why:**

- One-off operation triggered by an event.
- Should survive an application restart.
- Can be retried if the operation fails.
- The job is not a continuous process.

### Question 1:

Yes, there is no problem if the customer has to wait to receive the OTP because it may come from an external service provider, and it may take some time to process. Therefore, a `200 OK` response is returned, and the customer waits for the OTP to arrive.the job crashes after the text is sent, and the job crashes after the code is saved but before the text is sent.

### Question 2:

- Case 1:
If the job fails after the message has been sent, it is not sent again because it has already been saved. And if the job fails in general, the message is marked as expired.
- Case 2:
On retry, the message never sent at all because a record of it already exists in the database, so the function returns early.
Customer gets: no text. And "send again" does not help until the code expires, 5 minutes later.


---

## 5.2 Cancel Abandoned Orders

**Tool:** Hangfire Recurring Job

**Why:**

- Runs according to a schedule.
- Must survive application restarts.
- Should retry if execution fails.
- The operation should be safe to repeat.
- It processes persistent database state.

---

## 5.3 Cleanup Expired Refresh Tokens

**Tool:** Hangfire Recurring Job

**Why:**

- Runs according to a daily schedule.
- Must survive application restarts.
- Should retry if the database operation fails.
- It operates on persistent database data.

---

## 5.4 Create Daily Reports

**Tool:** Hangfire Recurring Job

**Why:**

- Runs once per day.
- Must survive application restarts.
- Should retry if report generation fails.
- The operation should be repeat-safe.
- Reports are stored persistently.

---

## 5.5 Retry a Failed Payment Capture

**Tool:** Hangfire

**Why:**

- The payment operation needs retry behavior.
- Retries should use backoff.
- The retries can continue for up to one day.
- The job must survive an application restart.
- The payment operation must be idempotent so that a retry does not accidentally capture the payment twice.

This is a good Hangfire use case because persistence and retry handling are important.

---

## 5.6 Continuously Drain an In-Memory Queue

**Tool:** `BackgroundService`

**Why:**

- The work is a continuous loop.
- It is tied to the lifetime of the application.
- The queue is stored in memory.
- The work does not need Hangfire's persistent job storage.

If the application restarts, the in-memory queue is lost, which makes Hangfire unnecessary for this specific scenario.

---

## 5.7 heartbeat 

**Tool:** `BackgroundService`

**Why:**

- The work is a continuous loop that runs for the lifetime of the application.
- It is not tied to a specific schedule or time of day (unlike a cron job).
- It only logs output and does not need to write to the database.

# 6. Additional Day 4 Work

## 6.1 Securing the Hangfire Dashboard

The Hangfire dashboard needs to be protected so that unauthorized users cannot access it.

Unauthorized users must be prevented from accessing the Hangfire Dashboard because they can use it to execute or disable jobs at any time and view the currently running and scheduled jobs.

This is dangerous because the dashboard provides direct control over background jobs and exposes information about the application's background processes.

The dashboard should be tested with:

- Admin → allowed.
- Partner → denied.
- Logged-out user → denied.

---

## 6.2 Cancel Abandoned Orders

The cancel job is more complicated because it modifies multiple pieces of state.

It needs to:

1. Find abandoned orders.
2. Lock the required rows.
3. Verify that the order is still `Pending`.
4. Cancel the order.
5. Return the reserved stock.
6. Add an order-status history record.
7. Commit everything as one transaction.

The implementation uses:

- Database transactions.
- `UPDLOCK`.
- `ROWLOCK`.
- Atomic stock updates.
- Concurrency handling.

The important principle is:


After fixing the code to prevent the Partner and the job from competing with each other, ROWLOCK was added, along with checking the order's status after retrieving it. This ensures that the order is only processed if it is still in the Pending status.
> Lock the state, check the state again, then make the change.

This prevents the cancellation job from cancelling an order that has already changed to another status.

---

# Conclusion

The main Hangfire concepts used in Day 4 are:

- **Fire-and-forget** for one-off persisted work.
- **Recurring jobs** for scheduled persisted work.
- **Continuation jobs** when one job depends on another job completing.
- **BackgroundService** for continuous application-lifetime processing.
- **Retries and persistence** when work must survive failures and restarts.
- **Idempotency** when a job can safely be executed more than once.

--- 

## Migration story 

What I emptied and why.
On Day 3 I created the migration AddIdempotensyKey, which added the IdempotencyKey column to the Order table with a unique index. EF generated the column as NOT NULL with a default value of an empty string (""). When the migration ran on a table that already had orders, every existing row received the same empty-string value, and creating the unique index failed because of the duplicates. I did not save the exact error text from that failure, so I am describing it from what I remember: a duplicate-key error on the unique index.

To get past the failure, I deleted everything inside Up() and changed the default value to a unique NEWID() generated by SQL Server.

Why that was a mistake.
A migration that is already committed must never be edited or emptied. A database that already ran it never runs it again, so the edit only affects databases that have not run it yet. My own database had already run the original version, so it still has the column and I never noticed a problem. A new database built from the repo runs the empty version, so the column is never created. The model and the snapshot still expect it, so the reseed fails with The given ColumnMapping does not match up with any column in the source or destination, and every order endpoint returns 500 with Invalid column name ‘IdempotencyKey’. Any database built from the repo since the Day 4 commit is affected, and putting the old text back would not fix those, because EF never runs a migration twice.

How the repair works.
I left AddIdempotensyKey.cs exactly as it is and added a new migration, RepairIdempotencyKey. Its Up() checks what each database already has and adds only what is missing, in four separate SQL steps:

Add the IdempotencyKey column as nullable, only if it does not exist.
Fill every order that has no key (or has an empty one) with its own NEWID() value, so the unique index cannot find duplicates. This is exactly the problem that broke the original migration.
Make the column NOT NULL, only if it is still nullable.
Create the unique index IX_Order_IdempotencyKey, only if it does not exist.

The four steps are separate Sql calls because SQL Server validates every column name in a batch before running it. If the UPDATE were in the same batch as the step that adds the column, the whole batch would be rejected on a database where the column does not exist yet. Down() is empty because this migration only repairs, and the column itself belongs to AddIdempotensyKey.

On both kinds of database.

My own database (column already exists): every check finds the work already done, so dotnet ef database update changes nothing.
A new empty database (column missing): dotnet ef database update adds the column, fills the keys, makes it required, and creates the unique index. Then the reseed finishes and the order endpoints return orders instead of a 500.


## Part-4
|**After Retry**|**After Crash**|**Stock Before**|**Product**|
|-|-|-|-|
|22|17|12|1|
|7|2|2|2|

**Order 30056 with productId =1 and quantity =5 / Order 30057 with productId =2 and quantity=5**


