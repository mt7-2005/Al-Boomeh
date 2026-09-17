# Day 03 — Incident Postmortems

---

## Incident #1: Oversold Stock (the classic)

In the first test, 50 orders were placed successfully even though the product quantity was only 5. This would have a major impact if more than one order is placed on the same product at the same time.

**Root Cause:** More than one transaction was able to read the old quantity value before it was updated.

**Detection:** Discovered through testing by sending 50 requests at the same time.

**Resolution:** The issue was solved using an Atomic update. The test was repeated and indeed 5 orders succeeded and 45 orders failed.

---

## Incident #2: Duplicate Order Codes

I thought this was already solved since day one, but after reconsidering and questioning it, it turned out to be correct but it caused an increase in database queries, which could cause delay and pressure.

**Resolution:** The query mechanism for checking the order during `place order` was changed to use:

```csharp
var order = await _context.Orders
    .Where(o => o.Id == orderId && o.Status == (int)enStatus.Holding)
    .Where(o => o.OrderCode != code)
    .FirstOrDefaultAsync();
```

This makes sure that no order has used this code before, and if one is found, a 400 error is returned.

---

## Incident #3: The OTP Guess-Limit Bypass (Day 2 door)

In the first test, the count never increased at all — `AttemptsUsed` stayed at 0, and the attacker could send many requests at the same time with nothing preventing that.

Then an atomic update was used, but the problem was not solved — `AttemptsUsed` ended up increasing far beyond 5, which is against what's required.

It turned out the reason is that a conditional update needs to be used, which is also considered an atomic update. So when a verify request was made 50 times at once, 5 of them returned `bad request` and 45 returned `too many requests`.

**Second test — two verifies with the correct code at the same moment:**

The first time, it returned `200, 200`, which is a big error — only one should succeed.

A row lock was used, which prevents any transaction from reading while another is reading, meaning no 2 transactions read the same data at the same time.

Repeating the test gave the result `200, 400`.

---

## Incident #4: Refresh-Token Rotation Race

On the first attempt to call 2 refresh tokens together, the response was `200, 200`.

This is wrong, because only one `200` should be returned, and reusing the token afterward should show it has expired.

Previous solutions did not work, so optimistic concurrency was used — it allows 2 transactions to read together, expecting they might both modify the data, and if one of them modifies it, the modification is rejected for the other. Repeating the test gave the result `200, 400`.

---

## Incident #5: Idempotency — The Double-Tap

In this case, if the customer clicks "create order" twice due to network delay, two orders get created, which is wrong.

So idempotency was used — a key sent with the request that is unique, and if it's repeated, order creation is rejected immediately, so only one order gets created. The `place order` is then done only on that single order that was created for the customer.

---

## Incident #6: Induce a Deadlock, Then Design It Out

This test did not succeed for me, even though I did `create order` => `place 2 orders` at the same time with reversed order on the products:
P1 => P2
P2 => P1

Repeated 20 times.

But a solution was put in place in case this happens in the future, by ordering the lines by ID so that no conflict occurs.
