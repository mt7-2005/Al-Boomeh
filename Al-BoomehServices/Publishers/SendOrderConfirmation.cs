using Al_BoomehDAL.Classes;
using Al_BoomehServices.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace Al_BoomehServices.Publishers
{
    public class SendOrderConfirmation: ISendOrderConfirmation, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private IChannel? _channel;
        private readonly SemaphoreSlim _channelLock = new(1, 1);



        private const string QueueName = "order-confirmation";



        public SendOrderConfirmation(IConnection connection)
        {
            _connection = connection;
        }

        public async Task SendConfirmation(SendOrderConfirmationDTO sendOrderConfirmationDTO)
        {
            await _channelLock.WaitAsync();
            try
            {
                if (_channel is null || _channel.IsClosed)
                {
                    _channel = await _connection.CreateChannelAsync();

                }


                var body = JsonSerializer.SerializeToUtf8Bytes(sendOrderConfirmationDTO);

                var props = new BasicProperties
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Persistent = true,
                    Headers = new Dictionary<string, object?>
                    {
                        ["version"] = 1
                    }
                };

                await _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: QueueName,
                    mandatory: false,
                    basicProperties: props,
                    body: body);
            }
            finally
            {
                _channelLock.Release();
            } 
        }
        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync();
            }
        }
    }

}
