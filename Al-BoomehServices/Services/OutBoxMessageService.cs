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
    public class OutBoxMessageService:IOutBoxMessageService
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
            var messages=await _context.OutboxMessages
                .Where(o=>o.SentAtUtc==null)
                .Select(m=>new MessageDto
                {
                    Id = m.Id,
                    MessageId = m.MessageId,
                    Version = m.Version,
                    Body=m.Payload,
                    Event=m.EventName
                })
                .OrderBy(o=>o.Id)
                .Take(50)
                .ToListAsync();

            return messages;
        }
        public async Task MarkMessageAsSent(int messageId)
        {
            var message=await _context.OutboxMessages
                .Where(o=>o.Id==messageId && o.SentAtUtc==null)
                .FirstOrDefaultAsync();

            if (message == null) throw new NotFoundException("No message found");

            message.SentAtUtc= DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            
       }
    }
}
