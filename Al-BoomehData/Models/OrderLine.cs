using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class OrderLine:BaseEntity, IAuditable
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = null;
    public decimal Price { get; set; }
    public float Quantity { get; set; }
    public int? ExtraId { get; set; }
    public decimal? ExtraPrice { get; set; }
    public string? ExtraName { get; set; }
    public decimal Total { get; set; }
    public bool IsCart {  get; set; }=false;
    public string? Notes { get; set; }

    public int OrderId { get; set; }

    

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
