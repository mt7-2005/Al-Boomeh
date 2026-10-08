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
        private readonly AppDbContext _context;
        private readonly ILogger<INotificationEngine> _logger;
        public LogNotificationEngine(AppDbContext context, ILogger<INotificationEngine> logger)
        {
            _context = context;
            _logger = logger;
        }
       
        public async Task SendAsync(string recipientEmail, string subject, string body)
        {
            _logger.LogInformation($"Send to email {recipientEmail} \n {subject}\n {body}");
        }
    }
}
