using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Models
{
   public class PushDevice : BaseEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string InstallationId { get; set; }
        public int Platform {  get; set; }
        public DateTime? LastSeenAtUtc { get; set; }
        public bool IsActive { get; set; }
    }
}
