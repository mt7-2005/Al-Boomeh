using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class ProductOption:BaseEntity
{
    public int Id { get; set; }

    public string ExtraName { get; set; } = null!;

    public bool IsMandatory { get; set; }

    public decimal? Price { get; set; }

    public int ProductId { get; set; }

   


    public virtual Product Product { get; set; } = null!;
}
