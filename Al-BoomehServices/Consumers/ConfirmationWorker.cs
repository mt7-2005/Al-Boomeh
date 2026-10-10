using Al_BoomehDAL.Classes;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Al_BoomehServices.Consumers
{
    public class ConfirmationWorker : BackgroundService
    {
        private const string QueueName = "order-confirmation";

        private const string RetryExchangeName = "order-confirmation.retry";
        private const string RetryQueueName = "order-confirmation.retry";
        private const string RetryRoutingKey = "retry";
        private const int RetryDelayMs = 10_000;

        private const string DlqName = "order-confirmation.dlq";
        private const int MaxAttempts = 3;

        private readonly IConnection _connection;
        private readonly IConfiguration _config;
        private readonly ILogger<ConfirmationWorker> _logger;
        private IChannel? _channel;

        public ConfirmationWorker(
            IConnection connection,
            ILogger<ConfirmationWorker> logger,
            IConfiguration configuration)
        {
            _connection = connection;
            _logger = logger;
            _config = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var port = _config["urls"] ?? $"pid {Environment.ProcessId}";
            var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            _channel = channel;

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

            await channel.BasicQosAsync(0, 0, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<SendOrderConfirmationDTO>(ea.Body.Span)!;

                    int delayMs = message.SequenceNumber % 3 == 1 ? 3000 : 200;

                    _logger.LogInformation(
                        "[{Port}] seq={Seq} start ({Delay}ms) OrderId {OrderId} CustomerId {CustomerId}",
                        port, message.SequenceNumber, delayMs, message.OrderId, message.CustomerId);

                    await Task.Delay(delayMs);

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

                    _logger.LogInformation(
                        "[{Port}] Confirmation sent for order {OrderId} (seq={Seq})",
                        port, message.OrderId, message.SequenceNumber);
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