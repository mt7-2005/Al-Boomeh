using Al_BoomehDAL.Classes;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Al_BoomehServices.Consumers
{
    public class NotificationsOrderPlaced : BackgroundService
    {
        private const string ExchangeName = "order-placed";
        private const string QueueName = "notification-order-placed";

        private const string RetryExchangeName = "order-placed.retry";
        private const string RetryQueueName = "notification-order-placed.retry";
        private const string RetryRoutingKey = "retry";
        private const int RetryDelayMs = 10_000;     


        private const string DlqName = "notification-order-placed.dlq";

        private const int MaxAttempts = 3;

        static int sequenceNumber;

        private readonly IConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationsOrderPlaced> _logger;
        private IChannel? _channel;

        public NotificationsOrderPlaced(
            IConnection connection,
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationsOrderPlaced> logger)
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            _channel = channel;

            await channel.ExchangeDeclareAsync(
                ExchangeName, ExchangeType.Fanout, durable: true,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                DlqName, durable: true, exclusive: false, autoDelete: false,
                cancellationToken: stoppingToken);

           
            await channel.ExchangeDeclareAsync(
                RetryExchangeName, ExchangeType.Direct, durable: true,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                RetryQueueName, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = RetryDelayMs,
                    ["x-dead-letter-exchange"] = "",                
                    ["x-dead-letter-routing-key"] = QueueName        
                },
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                RetryQueueName, RetryExchangeName, RetryRoutingKey,
                cancellationToken: stoppingToken);

           
            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = RetryExchangeName,
                    ["x-dead-letter-routing-key"] = RetryRoutingKey
                },
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: string.Empty,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var order = JsonSerializer.Deserialize<OrderPlacedEventDTO>(ea.Body.Span);

                    using var scope = _scopeFactory.CreateScope();
                    var engine = scope.ServiceProvider.GetRequiredService<INotificationEngine>();
                    var sendOrderConfirmation = scope.ServiceProvider.GetRequiredService<ISendOrderConfirmation>();

                    if (order?.OrderId == 999999999)
                    {
                        throw new Exception("Poison message test");
                    }

                    engine.MessagesEngine(message);
                    sequenceNumber++;
                    await sendOrderConfirmation.SendConfirmation(new SendOrderConfirmationDTO
                    {
                        SequenceNumber = sequenceNumber
                    });

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    var failedAttempts = GetFailedAttempts(ea.BasicProperties) + 1;

                    _logger.LogError(ex,
                        "Failed to process message {MessageId}, attempt {Attempt}/{Max}",
                        ea.BasicProperties.MessageId, failedAttempts, MaxAttempts);

                    try
                    {
                        if (failedAttempts >= MaxAttempts)
                        {
                            var props = new BasicProperties
                            {
                                Persistent = true,
                                MessageId = ea.BasicProperties.MessageId,
                                Headers = new Dictionary<string, object?>
                                {
                                    ["x-failed-attempts"] = failedAttempts,
                                    ["x-last-error"] = ex.Message
                                }
                            };

                            await channel.BasicPublishAsync(
                                exchange: "",
                                routingKey: DlqName,
                                mandatory: false,
                                basicProperties: props,
                                body: ea.Body);

                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        }
                        else
                        {
                           
                            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                        }
                    }
                    catch (Exception publishEx)
                    {
                        _logger.LogCritical(publishEx, "Failed to route failed message");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
            };

            await channel.BasicConsumeAsync(
                QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);
        }

      
        private static long GetFailedAttempts(IReadOnlyBasicProperties props)
        {
            if (props.Headers is null ||
                !props.Headers.TryGetValue("x-death", out var raw) ||
                raw is not List<object> deaths)
            {
                return 0;
            }

            foreach (var item in deaths)
            {
                if (item is not Dictionary<string, object> death) continue;

                var queue = AsString(death.GetValueOrDefault("queue"));
                var reason = AsString(death.GetValueOrDefault("reason"));

                if (queue == QueueName && reason == "rejected")
                {
                    return death.TryGetValue("count", out var count) ? Convert.ToInt64(count) : 0;
                }
            }

            return 0;
        }

        private static string? AsString(object? value) => value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string s => s,
            _ => null
        };

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync(cancellationToken: cancellationToken);
                _channel.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}