using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class CustomerCard:BaseEntity
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int CardId { get; set; }

   

    public virtual Card Card { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
