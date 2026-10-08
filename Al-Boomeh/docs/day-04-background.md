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

#### Create Daily Reports

- Runs once per day.
- Generates reports for stores.
- Must survive application restarts.
- Should be retryable if report generation fails.
- The operation should be repeat-safe to avoid creating duplicate reports.

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

Yes, there is no problem if the customer has to wait to receive the OTP because it may come from an external service provider, and it may take some time to process. Therefore, a `200 OK` response is returned, and the customer waits for the OTP to arrive.

### Question 2:

The code has been modified so that when a customer requests the OTP twice, the system checks the status of the previous OTP. If the previous OTP is still active, the job is terminated by using `return`, so Hangfire does not retry the job again.


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



