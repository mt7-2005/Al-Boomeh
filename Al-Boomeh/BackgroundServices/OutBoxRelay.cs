using Al_BoomehDAL.Classes;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Publishers;
using Al_BoomehServices.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehAPI.BackgroundServices
{
    public class OutBoxRelay : BackgroundService
    {
        private readonly ILogger<OutBoxRelay> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public OutBoxRelay(ILogger<OutBoxRelay> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var outBoxService = scope.ServiceProvider.GetRequiredService<IOutBoxMessageService>();
                        var publisher = scope.ServiceProvider.GetRequiredService<IOrderPlaced>();
                        var messages = await outBoxService.GetUnSentMessages();


                        foreach (var message in messages!)
                        {
                            await publisher.Publish(message);

                            // Test C — the relay dies halfway
                            //throw new Exception("message for test c");

                            await outBoxService.MarkMessageAsSent(message.Id);


                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "The outbox relay failed to send a batch.");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(5000), stoppingToken);
            }
        }
    }
}
