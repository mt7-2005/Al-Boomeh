using Al_BoomehDAL.Models;
using Al_BoomehServices.Dtos;
using Al_BoomehServices.Interfaces;
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
    public class OutBoxMessageService : IOutBoxMessageService, IOutBoxMessagesCleanUp
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OutBoxMessageService> _logger;

        public OutBoxMessageService(AppDbContext context, ILogger<OutBoxMessageService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<MessageDto>?> GetUnSentMessages()
        {
            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    DateTime now = DateTime.UtcNow;

                    var messages = await _context.OutboxMessages 
                      .FromSqlInterpolated($@"
                        SELECT TOP (50) *
                        FROM [OutboxMessages] WITH (UPDLOCK, ROWLOCK, READPAST)
                        WHERE SentAtUtc IS NULL AND FailedCount < 5
                        AND (ReadAtUtc IS NULL OR ReadAtUtc < DATEADD(second, -30, SYSUTCDATETIME()))
                        ORDER BY Id "
                    )
                      .ToListAsync();


                    var ids = messages.Select(m => m.Id).ToList();

                    await _context.OutboxMessages
                      .Where(m => ids.Contains(m.Id))
                      .ExecuteUpdateAsync(s => s.SetProperty(m => m.ReadAtUtc, now));

                    await transaction.CommitAsync();


                    return messages.Select(m => new MessageDto
                    {
                        Id = m.Id,
                        MessageId = m.MessageId,
                        Event = m.EventName,
                        Body = m.Payload,
                        Version = m.Version,
                    }).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to get OutBoxMessages" + ex.Message);
                    throw new Exception("Failed to get OutBoxMessages");
                }
            }
        }
        public async Task MarkMessageAsSent(int messageId)
        {
            var message = await _context.OutboxMessages
                .Where(o => o.Id == messageId && o.SentAtUtc == null)
                .FirstOrDefaultAsync();

            if (message == null) throw new NotFoundException("No message found");

            message.SentAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();


        }
        public async Task OutBoxMessagesCleanUp()
        {
            var now = DateTime.UtcNow;

            var messagesList =await _context.OutboxMessages
                .Where(m=>m.SentAtUtc!=null && m.CreatedAtUtc <= now.AddDays(-7))
                .ToListAsync();

            if (messagesList.Any())
            {
                _context.RemoveRange(messagesList);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} old messages .", messagesList.Count);
            }
            else
            {
                _logger.LogInformation("No old messages to delete.");
            }
        }
        public async Task UpdateErrorCount(int id,string errorMessage)
        {
            await _context.OutboxMessages
               .Where(m => m.Id == id)
               .ExecuteUpdateAsync(s => s
                   .SetProperty(m => m.FailedCount, m => m.FailedCount + 1)
                   .SetProperty(m => m.LastErrorMessage, errorMessage)
                   .SetProperty(m => m.ReadAtUtc,
                                m => m.FailedCount + 1 >= 5 ? m.ReadAtUtc : null));
               
        }
    }
}
