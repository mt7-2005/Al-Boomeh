using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Models
{
    public class Analytics
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public Store Store { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime LastUpdateUtc { get; set; }
    }
}
