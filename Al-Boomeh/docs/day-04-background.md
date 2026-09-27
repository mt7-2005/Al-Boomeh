# Hangfire Review — Day Summary

## 1. Job Shapes

### 1.1 Fire-and-forget

It is triggered when a specific event occurs and runs once.

It was used in `ISendOTP` when requesting an OTP code, where a Job is created to send the OTP when the request occurs.

### 1.2 Recurring Job

It runs periodically at a specific time or interval, with its schedule defined using a Cron Expression.

It was used for:
- `ITokenCleanup` to clean up Refresh Tokens.
- `IDailyReports` to generate daily reports for the stores.

### 1.3 Continuation Job

It runs after another Job finishes execution.

It was used to send notifications/reports to the stores after the daily report generation task finishes.

---

## 2. Cron Expressions

### 2.1 Cancel Abandoned Orders

```csharp
.AddOrUpdate<ICancelAbandonedOrders>(
    "cancel-abandoned-orders",
    x => x.CancelAbandonedOrders(),
    Cron.MinuteInterval(30));
```

**Meaning:** The task runs every 30 minutes.

---

### 2.2 Token Cleanup

```csharp
.AddOrUpdate<ITokenCleanup>(
    "token-cleanup",
    x => x.TokenCleanup(),
    "0 3 * * *");
```

**Meaning:** The task runs every day at `03:00 UTC`.

---

### 2.3 Daily Reports

```csharp
.AddOrUpdate<IDailyReports>(
    "daily-report",
    x => x.DailyReports(),
    "0 2 * * *");
```

**Meaning:** The task runs every day at `02:00 UTC`.

---

## 3. Retry / Idempotency — Double Refund

The Double Refund problem had already been handled before the test because the entire operation was executed inside a **Transaction**.

In addition, the following were used:
- Atomic Update
- Row Lock

Therefore, during the concurrency test, the inventory did not change incorrectly.

If the operation fails, the Transaction performs a `Rollback`, so none of the changes are applied to the database.

This made the operation **all-or-nothing**: either the entire operation succeeds, or all changes are rolled back.

---

## 4. BackgroundService vs Hangfire

### Hangfire

Hangfire was used when a task needed to run at a specific time or according to a schedule, especially when the task required:

- Retry in case of failure.
- Persisting the Job and scheduling information in the database.
- Keeping the Job persisted and processing it again after the application stops and starts again.
- Monitoring the Job status through the Dashboard.

### BackgroundService

`BackgroundService` differs from Hangfire because it is tied to the lifetime of the application.

If the application stops, the BackgroundService also stops.

If the task fails, there is no automatic Hangfire-style Retry, so the task does not automatically resume.

It is also not intended for scheduling in the same way as Hangfire, because it usually runs as a continuous process while the application is running.

---

## 5. Decision for the Six Scenarios

### 5.1 Send SMS OTP

Hangfire was used because it is triggered by a specific event and should run when that event occurs.

---

### 5.2 Cancel Abandoned Orders

Hangfire was used because it is a periodic task that runs every 30 minutes.

Hangfire also provides Retry in case execution fails.

The operation itself must be safe to repeat so that rerunning it does not apply the same change incorrectly.

---

### 5.3 Cleanup Token

Hangfire was used because it is a scheduled task that runs every day at `03:00 UTC`.

If execution fails, it can be retried.

---

### 5.4 Create Daily Report

Hangfire was used because it is a scheduled task that must run daily.

If it fails, it should be retryable, especially because another task depends on the report being generated successfully.

---

### 5.5 Send Reports

Hangfire was used because it depends on another task, which is the report generation task.

Therefore, a **Continuation Job** was used so that the reports are sent after the report generation task finishes.

---

### 5.6 Order Counts

`BackgroundService` was used because it is a periodic process that does not depend on another task and is tied to the application lifetime.

Therefore, using `BackgroundService` was more suitable for this case than Hangfire.

---

## 6. The Hardest Things Today

### 6.1 Securing the Hangfire Dashboard

One of the hardest things I faced was securing the Hangfire Dashboard.

The problem was that the Token was always empty when I tried to read it inside `HangFireAuthorizationFilter`.

After researching the issue, I found that the Hangfire Dashboard does not handle the API's JWT Authentication in the same way as the API endpoints, and that one commonly suggested solution is to use Cookie Authentication.

Therefore, this issue has not been fully resolved yet, and I still need to determine the best way to connect the Dashboard to the authentication already used in the project without relying on Cookie Authentication, if possible.

---

### 6.2 Cancel Abandoned Orders

`CancelAbandonedOrders` was also challenging because it depends on multiple tables.

I had to monitor the changes happening to the inventory and ensure that cancellation operations did not cause incorrect quantity changes.

I relied on the concepts learned and applied during the previous days, such as:

- Transactions
- Atomic Updates
- Row Locks
- Concurrency handling

These concepts helped make the inventory update process safe while handling concurrent operations.
