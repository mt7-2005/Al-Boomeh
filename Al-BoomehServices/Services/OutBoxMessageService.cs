using Al_BoomehDAL.Models;
using Al_BoomehServices.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;

namespace Al_BoomehServices.Services
{
    public class OutBoxMessageService : IOutBoxMessageService
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
                        WHERE SentAtUtc IS NULL AND ReadAtUtc IS NULL
                        ORDER BY Id"
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
                        Version = m.Version
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
    }
}
