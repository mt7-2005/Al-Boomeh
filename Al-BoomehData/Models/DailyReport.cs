using Al_BoomehDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class DailyReport 
{
    public int Id { get; set; }

    public int StoreId { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public bool IsSent {  get; set; }
    public DateOnly ReportDateUtc { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } 
}
