using Al_BoomehDAL.Models;
using Al_BoomehServices.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Services
{
    public class NotificationEngineService:INotificationEngine
    {
        private readonly AppDbContext _context;
        private readonly ILogger<INotificationEngine> _logger;
        public NotificationEngineService(AppDbContext context, ILogger<INotificationEngine> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task ReportsEngine()
        {
            var reports = await _context.DailyReports
                .Where(r => !r.IsSent)
                .ToListAsync();

            if (!reports.Any())
            {
                _logger.LogInformation("No new reports to send.");
                return;
            }

            foreach (var report in reports)
            {
                var sb = new StringBuilder();
                sb.AppendLine("\n--------------------------------------");
                sb.AppendLine($"\tStoreId: {report.StoreId}");
                sb.AppendLine($"\tTop Product: {report.ProductId}");
                sb.AppendLine($"\tOrders Count: {report.OrderCount}");
                sb.AppendLine($"\tTotal Revenue: {report.TotalRevenue}");
                sb.AppendLine("--------------------------------------");

                _logger.LogInformation(sb.ToString());

                report.IsSent = true;
            }

            await _context.SaveChangesAsync();
        }
        public void MessagesEngine(string message)
        {
            _logger?.LogInformation(message);
        }
    }
}
