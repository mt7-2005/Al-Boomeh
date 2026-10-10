using Al_BoomehDAL.Classes;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Al_BoomehServices.Consumers
{
    public class SavingAnalytics : BackgroundService
    {
        private const string ExchangeName = "order-placed";
        private const string QueueName = "saving-analytics-store";


        private const string RetryExchangeName = "saving-analytics-store.retry";
        private const string RetryQueueName = "saving-analytics-store.retry";
        private const string RetryRoutingKey = "retry";
        private const int RetryDelayMs = 10_000;

        private const string DlqName = "saving-analytics-store.dlq";
        private const int MaxAttempts = 3;

        private readonly IConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SavingAnalytics> _logger;
        private IChannel? _channel;

        public SavingAnalytics(
            IConnection connection,
            IServiceScopeFactory scopeFactory,
            ILogger<SavingAnalytics> logger)
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
                    var order = JsonSerializer.Deserialize<OrderPlacedEventDTO>(ea.Body.Span);

                    using var scope = _scopeFactory.CreateScope();
                    var analytics = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

                    if (order != null)
                    {
                        await analytics.UpdateAnalytic(
                            order.StoreId,
                            order.Total,
                            Guid.Parse(ea.BasicProperties.MessageId!));
                    }
                    
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    var failedAttempts = ReadAttempts.GetFailedAttempts(ea.BasicProperties, QueueName) + 1;

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