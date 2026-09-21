## Run it

1. Clone the repository.
2. Copy `Al-Boomeh/appsettings.Example.json` to `Al-Boomeh/appsettings.json`.
3. Open `appsettings.json` and fill in two values:
   - `ConnectionStrings:DefaultConnection` — your SQL Server connection string.
   - `Jwt:Key` — any random string of at least 32 characters.
4. Create the database:
   `dotnet ef database update --project Al-BoomehData --startup-project Al-Boomeh`
5. Fill it with data:
   `dotnet run --project Al-BoomehData -- --reseed`
6. Run the API:
   `dotnet run --project Al-Boomeh`
7. Open the Swagger page in your browser.

### Seeded accounts

**Admin**
- Email: `admin@al-boomeh.com`
- Password: `123456`

**Partner**
- Email: `partner@al-boomeh.com`
- Password: `123456`

### Finding a Customer OTP

When requesting an OTP for a customer, check the API logs.
It will show in the logs like:

```
SMS sent to 0780785454 The OTP : 545456
```
