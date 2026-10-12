## Run it

1. Clone the repository.
2. Copy `Al-Boomeh/appsettings.Example.json` to `Al-Boomeh/appsettings.json`.
3. Open `appsettings.json` and fill in three values:
   - `ConnectionStrings:DefaultConnection` — your SQL Server connection string.
   - `Jwt:Key` — any random string of at least 32 characters.
   - `RabbitMq:Uri` — see step 4 (use the password you choose there).
4. Start RabbitMQ:

```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=al-boomeh -e RABBITMQ_DEFAULT_PASS=<choose-a-password> rabbitmq:management
```

   - The two `-e` values only apply the first time a container starts. If a `rabbitmq` container already exists, run `docker rm -f rabbitmq` first. **This also deletes its queues.**
   - The management UI is at `http://localhost:15672`. Log in as `al-boomeh` with the password you chose.
   - The Uri is `amqp://al-boomeh:<your-password>@localhost:5672/`. Put it in `RabbitMq:Uri` in `appsettings.json`.
5. Create the database:
   `dotnet ef database update --project Al-BoomehData --startup-project Al-Boomeh`
6. Fill it with data:
   `dotnet run --project Al-BoomehData -- --reseed`
7. Run the API:
   `dotnet run --project Al-Boomeh`
8. Open the Swagger page in your browser and log in as the seeded Admin (see "Seeded accounts" below).
9. Open the Hangfire dashboard in the **same browser**: `https://localhost:7027/hangfire`
   - Only an Admin's login sets the cookie, so step 8 must come first.
   - The port `7027` may differ depending on your `launchSettings.json`.

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

    SMS sent to 0780785454 The OTP : 545456