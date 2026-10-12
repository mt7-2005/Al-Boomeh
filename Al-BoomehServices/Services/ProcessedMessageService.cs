using Al_BoomehDAL.Models;
using Al_BoomehServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Al_BoomehServices.Services
{
    public class ProcessedMessageService : IProcessedMessageService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProcessedMessageService> _logger;

        public ProcessedMessageService(AppDbContext context, ILogger<ProcessedMessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> TryAdd(Guid messageId, string consumer)
        {
            _context.ProcessedMessages.Add(new ProcessedMessage { MessageId = messageId, Consumer = consumer });

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlException
                      && (sqlException.Number == 2601 || sqlException.Number == 2627))
            {
                _logger.LogInformation("Message {MessageId} was already processed by {Consumer}. Skipping it.", messageId, consumer);
                return false;
            }
        }

        public async Task Remove(Guid messageId, string consumer)
        {
            var processedMessage = await _context.ProcessedMessages
                .FirstOrDefaultAsync(p => p.MessageId == messageId && p.Consumer == consumer);

            if (processedMessage != null)
            {
                _context.ProcessedMessages.Remove(processedMessage);
                await _context.SaveChangesAsync();
            }
        }
    }
}
