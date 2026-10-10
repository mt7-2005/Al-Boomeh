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
8. Open HangFire dashboard:
    `it open only for admin `
    `open https://localhost:7027/hangfire port 7027 may differ depending on your launchSettings.json configuration `
      
9. Run RabbitMQ :
    `run the comand  docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management`
    `open http://localhost:15672 and update "RabbitMq" link in appsettings like "Uri": "amqp://al-boomeh:123456@localhost:567%2f"`

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
