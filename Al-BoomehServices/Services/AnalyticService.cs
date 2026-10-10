using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Services
{
    public class AnalyticService: IAnalyticsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnalyticService> _logger;

        public AnalyticService(AppDbContext context, ILogger<AnalyticService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> UpdateAnalytic(int storeId, decimal totalRevenue, Guid messageId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.ProcessedMessages.Add(new ProcessedMessage { MessageId = messageId, Consumer = "analytics" });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlException
                      && (sqlException.Number == 2601 || sqlException.Number == 2627))
            {
                _logger.LogInformation("Message {MessageId} was already processed. Skipping it.", messageId);
                return false;
            }

            var store = await _context.Analytics.FirstOrDefaultAsync(s => s.StoreId == storeId);

            if (store != null)
            {
                store.TotalRevenue += totalRevenue;
                store.OrderCount++;
                store.LastUpdateUtc = DateTime.UtcNow;
            }
            else
            {
                _context.Analytics.Add(new Analytics
                {
                    StoreId = storeId,
                    TotalRevenue = totalRevenue,
                    OrderCount = 1,
                    LastUpdateUtc = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Analytics updated for store with Id {StoreId}", storeId);
            return true;
        }

        public async Task<AnalyticsDTO?> GetAnalytics(int storeId)
        {
            var analytic=await _context.Analytics
                .AsNoTracking()
                .Where(x=>x.StoreId==storeId)
                .Select(a=> new AnalyticsDTO
                {
                    StoreId=a.StoreId,
                    TotalRevenue=a.TotalRevenue,
                    OrderCount=a.OrderCount,
                    LastUpdateUtc=a.LastUpdateUtc
                }).FirstOrDefaultAsync();

            return analytic ?? throw new NotFoundException($"Store {storeId} has no sales yet.");
        }

        public async Task<List<AnalyticsDTO>?> GetAllAnalytics()
        {
            var analyticsList=await _context.Analytics
                .AsNoTracking()
                .Select(a => new AnalyticsDTO
                {
                    StoreId = a.StoreId,
                    TotalRevenue = a.TotalRevenue,
                    OrderCount = a.OrderCount,
                    LastUpdateUtc = a.LastUpdateUtc
                }).ToListAsync();
            return analyticsList;
        }
    }
}
