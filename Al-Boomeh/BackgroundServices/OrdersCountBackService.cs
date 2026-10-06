using Al_BoomehDAL.Classes;
using Al_BoomehServices.Interfaces;
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
    public class OrdersCountBackService:BackgroundService
    {
        private readonly ILogger<OrdersCountBackService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public OrdersCountBackService(ILogger<OrdersCountBackService> logger, IServiceScopeFactory scopeFactory)
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
                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                        var ordersCount = await orderService.GetOrderCountPairsStatus();

                        _logger.LogInformation(
                            "Pending :{penOrders} || Accepted: {acceptedOrders} || Preparing: {preOrders} || OutForDelivery: {shippingOrders}",
                             ordersCount.GetValueOrDefault(enStatus.Pending, 0),
                             ordersCount.GetValueOrDefault(enStatus.Accepted, 0),
                             ordersCount.GetValueOrDefault(enStatus.Preparing, 0),
                             ordersCount.GetValueOrDefault(enStatus.OutForDelivery, 0));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get orders count");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
