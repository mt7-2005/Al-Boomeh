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

namespace Al_Boomeh.BackgroundServices
{
    public class OrderConsumer: BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                }
                catch (Exception ex)
                {
                    
                }

               // await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
