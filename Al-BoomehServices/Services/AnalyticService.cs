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

        public async Task<bool> UpdateAnalytic(int storeId,decimal totalRevenue,Guid messageId)
        {

            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                var message = new ProcessedMessage
                {
                    MessageId = messageId
                };
                await _context.AddAsync(message);

                var store = await _context.Analytics
                    .FirstOrDefaultAsync(s => s.StoreId == storeId);

                if (store != null)
                {
                    store.TotalRevenue += totalRevenue;
                    store.OrderCount++;
                    store.LastUpdateUtc = DateTime.UtcNow;


                }
                else
                {
                    var newStore = new Analytics()
                    {
                        StoreId = storeId,
                        TotalRevenue = totalRevenue,
                        OrderCount = 1,
                        LastUpdateUtc = DateTime.UtcNow
                    };
                    await _context.AddAsync(newStore);
                }

                int rowsEffect = await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                if (rowsEffect > 0)
                {
                    _logger.LogInformation("Analytics updated for store with Id {StoreId}", storeId);
                    return true;
                }
            }
            catch(DbUpdateException ex)
              when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlException
            && (sqlException.Number == 2601 || sqlException.Number == 2627))
            {
                await transaction.RollbackAsync();
                throw new BusinessRuleException("Failed to add analytics");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            return false;
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

            return analytic;
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
