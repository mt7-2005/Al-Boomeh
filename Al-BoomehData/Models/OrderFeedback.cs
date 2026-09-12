using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class OrderFeedback:BaseEntity
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int Rate { get; set; }

    public string? Notes { get; set; }

    

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
