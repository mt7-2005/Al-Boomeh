using Al_BoomehDAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Jobs
{
    public class LogNotificationEngine:INotificationEngine
    {
        private readonly ILogger<INotificationEngine> _logger;
        public LogNotificationEngine( ILogger<INotificationEngine> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string recipientEmail, string subject, string body)
        {
            _logger.LogInformation("Notification to {RecipientEmail}: {Subject} {Body}", recipientEmail, subject, body);
            return Task.CompletedTask;
        }

    }
}
