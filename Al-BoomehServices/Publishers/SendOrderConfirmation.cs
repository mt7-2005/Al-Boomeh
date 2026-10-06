using Al_BoomehDAL.Classes;
using Al_BoomehServices.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Al_BoomehServices.Publishers
{
    public class SendOrderConfirmation: ISendOrderConfirmation
    {
        private readonly IConnection _connection;

        private const string QueueName = "order-confirmation";

        public SendOrderConfirmation(IConnection connection)
        {
            _connection = connection;
        }

        public async Task SendConfirmation(SendOrderConfirmationDTO sendOrderConfirmationDTO)
        {
            await using var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var message = new
            {
                SequenceNumber = sendOrderConfirmationDTO.SequenceNumber,
            };

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            var props = new BasicProperties
            {
                MessageId = Guid.NewGuid().ToString(),
                Persistent = true,
                Headers = new Dictionary<string, object?>
                {
                    ["version"] = 1
                }
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: QueueName,
                mandatory: false,
                basicProperties: props,
                body: body);
        }
    }
}
