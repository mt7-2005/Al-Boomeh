using Al_BoomehDAL.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Al_BoomehDAL.Seeding.Seeders
{
    public class Payload
    {
        public int OrderId { get; set; }
        public int StoreId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public decimal Total {  get; set; }
    }

    public class OutBoxMessagesSeeder
    {
        public static List<OutBoxMessage> Generate(int count=200000)
        {
           var faker = new Faker();
           

            var outMessagesList= new List<OutBoxMessage>();

            for (int i = 0; i < count; i++)
            {
                var createdAtUtc = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 30));

                var payload = new Payload()
                {
                    OrderId = i,
                    StoreId = faker.Random.Int(1, 50),
                    CustomerId = faker.Random.Int(1, 20000),
                    CreatedAtUtc = createdAtUtc,
                    Total = faker.Random.Decimal(1, 50)

                };
                outMessagesList.Add(new OutBoxMessage()
                {
                    MessageId = faker.Random.Guid(),
                    EventName = "OrderPlaced",
                    CreatedAtUtc = createdAtUtc,
                    Payload = JsonSerializer.Serialize(payload),
                    SentAtUtc = DateTime.UtcNow,
                    ReadAtUtc = DateTime.UtcNow,
                    FailedCount= 0,
                    Version= 1
                });
            }
            return outMessagesList;
        }
    }
}