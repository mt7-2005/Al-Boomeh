using Al_Boomeh.Hubs;
using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
namespace Al_BoomehServices.Consumers
{
    public class OrderPushConsumer : BackgroundService
    {
        private const string ExchangeName = "order-status-changed";
        private const string PlacedExchangeName = "order-placed";


        private const string RetryExchangeName = "order-status-changed.retry";

        private const int RetryDelayMs = 10_000;
        private string _queueName = string.Empty;


        private const string DlqName = "status-changed.dlq";
        private readonly string _consumerName = $"order-push:{Guid.NewGuid():N}";
        private const int MaxAttempts = 3;

        private readonly IHubContext<OrdersHub> _hub;
        private readonly IConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderPushConsumer> _logger;
        private IChannel? _channel;

        public OrderPushConsumer(
            IConnection connection,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderPushConsumer> logger,
            IHubContext<OrdersHub> hub)
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hub = hub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            _channel = channel;

            _queueName = $"order-status-changed.{Guid.NewGuid():N}";
            var retryQueue = $"{_queueName}.retry";

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
                retryQueue, durable: false, exclusive: true, autoDelete: true,
                arguments: new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = RetryDelayMs,
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = _queueName
                },
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                retryQueue, RetryExchangeName, _queueName,
                cancellationToken: stoppingToken);


            await channel.QueueDeclareAsync(
                queue: _queueName,
                durable: false,
                exclusive: true,
                autoDelete: true,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = RetryExchangeName,
                    ["x-dead-letter-routing-key"] = _queueName
                },
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(_queueName, ExchangeName, routingKey: "",
                cancellationToken: stoppingToken);

            await channel.BasicQosAsync(0, 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var messageId = Guid.Parse(ea.BasicProperties.MessageId!);
                    var order = JsonSerializer.Deserialize<OrderStatusChangedDto>(ea.Body.Span)
                        ?? throw new InvalidOperationException("The message has no OrderStatusChanged body.");

                    using var scope = _scopeFactory.CreateScope();
                    var processedMessages = scope.ServiceProvider.GetRequiredService<IProcessedMessageService>();

                    if (!await processedMessages.TryAdd(messageId, _consumerName))
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    try
                    {
                        var update = new { OrderId = order.OrderId, Status = order.NewStatus.ToString() };

                        await _hub.Clients
                            .Group($"order-{order.OrderId}")
                            .SendAsync("OrderStatusChanged", update);

                    }
                    catch
                    {
                        await processedMessages.Remove(messageId, _consumerName);
                        throw;
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    var failedAttempts = ReadAttempts.GetFailedAttempts(ea.BasicProperties, _queueName) + 1;

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
                _queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            var placedQueue = $"order-placed-push.{Guid.NewGuid():N}";

            await channel.ExchangeDeclareAsync(
                PlacedExchangeName, ExchangeType.Fanout, durable: true,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                placedQueue, durable: false, exclusive: true, autoDelete: true,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(placedQueue, PlacedExchangeName, routingKey: "",
                cancellationToken: stoppingToken);

            var placedConsumer = new AsyncEventingBasicConsumer(channel);
            placedConsumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var placed = JsonSerializer.Deserialize<OrderPlacedEventDTO>(ea.Body.Span)
                        ?? throw new InvalidOperationException("The message has no OrderPlaced body.");

                    await _hub.Clients
                        .Group($"store-{placed.StoreId}")
                        .SendAsync("OrderPlaced", new { orderId = placed.OrderId });

                    _logger.LogInformation(
                        "Pushed OrderPlaced: order {OrderId} to store {StoreId}",
                        placed.OrderId, placed.StoreId);

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to push OrderPlaced {MessageId}", ea.BasicProperties.MessageId);
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await channel.BasicConsumeAsync(
                placedQueue,
                autoAck: false,
                consumer: placedConsumer,
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