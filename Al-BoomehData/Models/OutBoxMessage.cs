using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Models
{
    public class OutBoxMessage : BaseEntity
    {
        public int Id { get; set; }
        public Guid MessageId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Payload { get; set; } = string.Empty;
        public DateTime? SentAtUtc { get; set; }
        public int FailedCount { get; set; }
        public string LastErrorMessage { get; set; } = string.Empty;

    }
}
