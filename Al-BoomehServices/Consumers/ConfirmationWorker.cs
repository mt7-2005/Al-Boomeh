using Al_BoomehDAL.Classes;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Jobs;
using Al_BoomehServices.Publishers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


namespace Al_BoomehServices.Consumers
{
    public class ConfirmationWorker : BackgroundService
    {
        private const string QueueName = "order-confirmation";


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
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

           

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);


            await _channel.BasicQosAsync(0, 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<SendOrderConfirmationDTO>(ea.Body.Span)!;

                    int delayMs = message.SequenceNumber % 3 == 1 ? 3000 : 200;

                    _logger.LogInformation("{Time:HH:mm:ss.fff} [{Port}] seq={Seq} start ({Delay}ms)",
                DateTime.Now,port, message.SequenceNumber, delayMs);

                    await Task.Delay(delayMs);

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

                    _logger.LogInformation("{Time:HH:mm:ss.fff} [{Port}] seq={Seq} done",
                        DateTime.Now, port, message.SequenceNumber);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message {MessageId}", ea.BasicProperties.MessageId);

                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await _channel.BasicConsumeAsync(
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