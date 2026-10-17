using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
       public string Body { get; set; }
        public Guid MessageId { get; set; }
        public int Version { get; set; }
        public string Event {  get; set; }
    }
}
