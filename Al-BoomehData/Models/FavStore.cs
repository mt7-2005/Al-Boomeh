using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class FavStore:BaseEntity
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public int CustomerId { get; set; }

    
    public virtual Customer Customer { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
