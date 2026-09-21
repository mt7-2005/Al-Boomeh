# User & Authentication Design

## User Model

```text
User
 ├── Id (Guid)
 ├── Role
 ├── Email
 ├── Password
 ├── CustomerId
 └── StoreId
```

## Identity Model

A user can be associated with:

- Customer
- Store / Partner
- Admin role

If `CustomerId = null` and `StoreId = null`, then the role is **Admin**.

Login methods:

- **Partner and Admin:** log in using email and password.
- **Customer:** requests an OTP using their phone number and verifies it to log in.

## Why Guid?

User IDs use `Guid` instead of sequential integers because they provide globally unique identifiers and are harder to predict than sequential IDs.

This is useful for identity records where predictable IDs are undesirable.

## Password Hashing

Argon2id is intentionally computationally expensive and memory-intensive. This makes large-scale password guessing and brute-force attacks more expensive for an attacker.

Password hashing should be slow enough to increase the cost of trying many password guesses.

## OTP Design

The customer requests an OTP using their phone number.

The OTP:

- has a limited lifetime
- is stored server-side
- can only be used once
- is marked as used after successful verification
- is not regenerated while an existing valid, unused OTP is still available (this prevents unnecessary generation of another OTP)

The OTP flow also tracks verification attempts to limit repeated incorrect verification attempts.

> **Note:** If the OTP appears in the logs, it means it is for testing only.

## JWT Signing Secret

The JWT signing secret is not hard-coded in source control.

Production secrets should be provided through secure, environment-specific secret management rather than committed to the repository.

## Why Seeding Stayed Fast

Password hashing does not scale with the number of rows in my seed. Every seeded user gets the same password (`123456`), so it is hashed once with Argon2id and that hash is reused for all 20,051 user rows. Hashing each row separately would have been far slower, because Argon2id is deliberately expensive. In real use only the 51 non-customer accounts (partners and the admin) ever log in with a password, since customers log in with an OTP. The whole `Users` table was seeded in 249 ms.

## Ownership Checks

### Single home

Store ownership is checked in one place: `StoreOwnerOrAdminHandler`, an ASP.NET Core authorization handler (`AuthorizationHandler<StoreOwnerOrAdminRequirement, int>`) where the resource is the `storeId` being accessed. Admins pass. Everyone else must have `user.StoreId == storeId`, where the user is loaded from the database. Endpoints that touch a store's data go through this handler instead of comparing ids themselves, so a new endpoint cannot skip the check by writing its own `if`.

Used by: **TODO: list of endpoints that use this handler.**

Customer ownership (for example `Addresses`, `Customers`, `Orders`) is checked by: **TODO: name of the handler or method.**

### Token claim vs database

The token is trusted only for who the caller is: its signature is verified, and the user id (`NameIdentifier`) inside it is used to load the user from the database. The store is not read from a token claim at all. `StoreId` comes from the database record, so removing or reassigning a partner takes effect on the next request instead of when the token expires. The client never chooses which store it belongs to; it can only name the store it wants to access, and that is compared with the database value.

The one thing still taken from the token is the `Admin` role (`IsInRole("Admin")`). A demoted admin therefore keeps admin access until their access token expires (**TODO: access token lifetime**).

## Attack Log Summary

I ran 12 attacks covering authentication, authorization and abuse prevention. All 12 behaved as expected. Full details, with every endpoint tried and its status code, are in [`docs/attack-log.md`](attack-log.md). Last run: **TODO: date**, commit **TODO: hash**.

| # | Attack | Expected | Result |
|---|---|---|---|
| 1 | No token on a protected endpoint | `401` | Pass |
| 2 | Garbage token | `401` | Pass |
| 3 | Cross-customer access (BOLA) | `403` | Pass |
| 4 | Partner edits another store's product | `403` | Pass |
| 5 | Tampered JWT claim, not re-signed | `401` | Pass |
| 6 | JWT `alg: none` | `401` | Pass |
| 7 | Expired access token, then refresh | `401`, then success | Pass |
| 8 | Reused rotated refresh token | `401`, all sessions revoked | Pass |
| 9 | OTP brute force | Blocked early | Pass (blocked at attempt 5) |
| 10 | OTP reuse | `400` | Pass |
| 11 | OTP request flooding | `429` | Pass |
| 12 | Account enumeration | No visible difference | Pass |

## Hardest Problem

The hardest problem was the OTP verification. It was something new to me, and I had to check whether the code was correct and whether it was still active, and with every wrong attempt the number of attempts had to be increased.

The problem was solved after trying several times and testing more than one approach.
