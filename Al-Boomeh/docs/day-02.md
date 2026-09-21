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

## Hardest Problem

The hardest problem was the OTP verification. It was something new to me, and I had to check whether the code was correct and whether it was still active, and with every wrong attempt the number of attempts had to be increased.

The problem was solved after trying several times and testing more than one approach.
