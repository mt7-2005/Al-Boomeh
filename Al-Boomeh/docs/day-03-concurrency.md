# Concurrency & Data Consistency Concepts

## Race Condition

Occurs when more than one process tries to read or modify the same object at the same time.

**Example from the project:** When multiple `place order` requests are executed simultaneously, all of them read the *old* value of the product's `stock quantity` before any of them commits a change. As a result, every request ends up succeeding — even though the actual stock should not allow that.

## Lost Update

Happens when more than one operation runs at the same time on the same object: one update succeeds while the other doesn't, but both operations originally read the *same old (stale) data* before either write was applied — so the second write can silently overwrite/ignore the effect of the first.

**Example from the project:** Before using an atomic update, the `verify OTP` endpoint allowed more than 5 attempts, even though the maximum allowed was 5.

## Transactions and Atomicity

**Transactions:** A group of operations executed as a single unit of work. If any operation within the group fails, none of the changes are applied — i.e., all operations either succeed together or fail together.
*Used in:* `place order` and `create order`.

**Atomicity:** Updating data based on its most recently read value, where the read and the update happen as a single, indivisible operation.
*Example:* When updating a product's stock, the latest value is read and updated directly in the same operation, without any intermediate step where another process could interleave.

## Read Committed

Allows two transactions to read data concurrently, but neither transaction can read data that has been modified by another transaction and not yet committed. This is the **default isolation level** for the database currently in use.

*Used in:* `refresh token`, implemented using optimistic concurrency.

## Snapshot Isolation

Instead of allowing transactions to read the database concurrently in a way that can conflict, each transaction reads from a **consistent snapshot** of the data taken at a specific point in time.

## Pessimistic Concurrency

Allows only one transaction to access/modify the data at a time. Any other transaction attempting to access the same data is **blocked** until the first transaction finishes, since a conflict is assumed to be likely.

## Optimistic Concurrency

Allows two transactions to proceed concurrently, assuming that a conflict is unlikely. If a conflict does occur, the second transaction fails and is retried, while the first transaction's changes are committed as-is.

## Atomic Single-Statement Updates

An update where the read of the existing value and the update itself happen within a **single statement**.

*Used in:* Updating product stock when placing an order.

## Unique Constraints as the Last Line of Defense

A unique constraint was applied on `OrderCode`. Previously (since Day 01), it was protected using **recursion**. This was later changed to use the following approach instead:

```csharp
.Where(o => o.Id == orderId && o.Status == (int)enStatus.Holding)
.Where(o => o.OrderCode != code)
```

If no matching order is found:

```csharp
if (order == null)
    throw new BusinessRuleException($"Failed to place order");
```

## Idempotency

When two identical requests are sent at the same time, each request includes a unique `IdempotencyKey`, which is stored in the database. If a new request arrives with a key that has already been used, it is **rejected**.
