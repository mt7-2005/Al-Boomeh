using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Voucher:BaseEntity
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string Code { get; set; } = null!;

    public DateTime ExpirationDate { get; set; }

    public int CustomerId { get; set; }

    public decimal MinimumAmount { get; set; }

    public decimal MaximumDiscount { get; set; }

    public bool IsUsed { get; set; }

   

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
