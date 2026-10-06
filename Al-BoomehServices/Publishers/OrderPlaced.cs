using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Al_BoomehDAL.Classes;
using RabbitMQ.Client;
using System.Text.Json;
using Al_BoomehServices.Interfaces;


namespace Al_BoomehServices.Publishers
{
    public class OrderPlaced:IOrderPlaced
    {
        private readonly IConnection _connection;
        public OrderPlaced(IConnection connection)
        {
            _connection = connection;
        }

        public async Task Publish(OrderPlacedEventDTO orderDto)
        {
            await using var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync("order-placed",
                ExchangeType.Fanout,
                 durable: true);
            var testOrder = new OrderPlacedEventDTO
            {
                OrderId = 999999999,
                StoreId = 1,
                CustomerId = 1,
                Total = 50,
                CreatedAtUtc = DateTime.UtcNow
            };

            var message= JsonSerializer.Serialize(testOrder);

            var body = Encoding.UTF8.GetBytes(message);

            var messageId = Guid.NewGuid().ToString();

            var props = new BasicProperties
            {
                MessageId = messageId,
                Persistent = true,
                Headers = new Dictionary<string, object?>
                {
                    ["version"] = 1
                }
            };

            await channel.BasicPublishAsync(
                exchange: "order-placed",
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: props,
                body: body);

        }
    }
}
