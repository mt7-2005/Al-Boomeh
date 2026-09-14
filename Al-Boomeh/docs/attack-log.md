# API Security Test Checklist

A record of authentication, authorization, and abuse-prevention tests performed against the API, along with observed results.

---

## 1. Unauthenticated Request to Protected Endpoint
Call a protected endpoint (e.g. order details) with no token. Expect `401`.

- **Input:** `55`
- **Endpoint:** `https://localhost:7027/api/Addresses/55`
- **Response:** `401` ✅

---

## 2. Garbage Token
Call it with a garbage token (`Bearer abc.def.ghi`). Expect `401`.

- **Input:** `$2a$11$EbJgQeWwUlwqaJStQ.PEie01oi.TralaTKTvXz49RUYPUt2bP.FKi`
- **Endpoint:** `https://localhost:7027/api/Auth/me`
- **Response:** `401` ✅

---

## 3. Broken Object-Level Authorization (BOLA) — Cross-Customer Access
Log in as Customer A. Use A's valid token to fetch Customer B's order by id. Expect `403`.

> If you get B's data — that's the single most common real-world API breach. Fix it before moving on.

- **Input:** Customer A `Id=1` / Customer B `Id=2`
- **Endpoint:** `https://localhost:7027/api/Customers/1/get-customers-orders?pageNumber=1&pageSize=3`
- **Response (A):** `200` ✅
- **Response (B):** `403` ✅

---

## 4. Cross-Store Authorization — Partner Editing Another Store's Product
Log in as the Partner of Store 1. Try to edit a product of Store 2. Expect `403`.

- **Input:** Store 1 `id=42` / Store 2 `id=5` / Product `id=3424`
- **Response:** `403` ✅

---

## 5. Tampered JWT Claims (No Re-Signing)
Take a valid token and tamper with a claim — flip your role to Admin, or change the user id — without re-signing. Expect `401` (signature check catches it).

- **Input:** Role tampered `Partner → Admin`
- **Response:** `401` ✅

---

## 6. JWT `alg: none` Attack
Research the `alg: none` attack (attacker sets the token's algorithm header to `"none"` to skip signature checking). Craft one and fire it. Expect rejection.

> If your validation accepts it, you have a critical hole — understand why and fix it.

- **Input:** `"alg": "none"`
- **Endpoint:** `https://localhost:7027/api/Addresses/55`
- **Response:** `401` ✅

---

## 7. Access Token Expiry + Refresh Flow
Let an access token expire, then call a protected endpoint. Expect `401`. Then use refresh to get a new one and succeed.

- **First response:** `401` ✅
- **After refresh:** Succeeded ✅

---

## 8. Refresh Token Reuse (Rotation Check)
Reuse a refresh token you already rotated away. Expect rejection and the whole chain revoked.

- **Response:** `401` ✅

---

## 9. OTP Brute-Force Protection
Request a code for a phone, then submit wrong codes in a loop.

> You must be blocked long before you could try a meaningful fraction of the million possibilities — by attempt-limiting AND by expiry. If you can keep guessing, the OTP door is wide open, and a million tries is nothing to a script.

- **Blocked at:** Attempt #5
- **Verify response:** `400` ✅

---

## 10. OTP Reuse (Single-Use Enforcement)
Verify successfully, then submit the same code again. Expect rejection — it must be single-use.

- **Response:** `400` ✅

---

## 11. OTP Request Flooding / Rate Limiting
Call request-OTP for one phone many times fast. You should be throttled; otherwise you're a free tool for spamming someone's phone and burning SMS cost.

- **Response:** `429` ✅

---

## 12. Account/User Enumeration Probe
Does `request-OTP` (or `verify`, or staff login) answer differently for a real vs. a made-up phone/email — different message, different timing, different status?

> Any visible difference lets an attacker map out who has an account. All of them should stay tight-lipped.

- **Endpoint:** `https://localhost:7027/api/Auth/RequestOtp`
- **Try 1:** `0799858933` (exists in database) → **Response:** `200`
- **Try 2:** `5454545454` (random number) → **Response:** `200`
- **Result:** No visible difference between real and fake numbers ✅

---

## Summary

| # | Test | Expected | Result |
|---|------|----------|--------|
| 1 | No token | 401 | ✅ Pass |
| 2 | Garbage token | 401 | ✅ Pass |
| 3 | Cross-customer access (BOLA) | 403 | ✅ Pass |
| 4 | Cross-store product edit | 403 | ✅ Pass |
| 5 | Tampered claim, no re-sign | 401 | ✅ Pass |
| 6 | `alg: none` attack | 401 | ✅ Pass |
| 7 | Expired token + refresh | 401 → success | ✅ Pass |
| 8 | Reused rotated refresh token | 401 | ✅ Pass |
| 9 | OTP brute-force | Blocked early | ✅ Pass (blocked at #5) |
| 10 | OTP reuse | 400 | ✅ Pass |
| 11 | OTP request flooding | 429 | ✅ Pass |
| 12 | Account enumeration | No difference | ✅ Pass |
