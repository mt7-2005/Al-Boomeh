using Al_BoomehDAL.Classes;
using Al_BoomehServices.Dtos;
using Al_BoomehServices.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Al_BoomehServices.Publishers
{
    public class OrderPlaced : IOrderPlaced, IAsyncDisposable
    {
        private const string ExchangeName = "order-placed";

        private readonly IConnection _connection;
        private readonly SemaphoreSlim _channelLock = new(1, 1);
        private IChannel? _channel;

        public OrderPlaced(IConnection connection)
        {
            _connection = connection;
        }

        public async Task Publish(MessageDto message)
        {
            await _channelLock.WaitAsync();
            try
            {
                if (_channel is null || _channel.IsClosed)
                {
                    _channel = await _connection.CreateChannelAsync();
                    await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout, durable: true);
                }

                var props = new BasicProperties
                {
                    MessageId = message.MessageId.ToString(),
                    Type = "OrderPlaced",
                    Persistent = true,
                    Headers = new Dictionary<string, object?>
                    {
                        ["version"] = message.Version
                    }
                };
                var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message.Body));

                await _channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: string.Empty,
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