using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class OrderStatusHistory:BaseEntity
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int OldStatus { get; set; }

    public int NewStatus { get; set; }


   

    public virtual Order Order { get; set; } = null!;
}
