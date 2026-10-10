using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Models
{
    public class ProcessedMessage
    {
        public int Id { get; set; } 
        public Guid MessageId { get; set; }
        public string Consumer { get; set; } = null!;

    }
}

