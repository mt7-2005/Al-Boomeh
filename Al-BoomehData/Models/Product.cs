using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Product: BaseEntity
{
    public int Id { get; set; }
    public string ProductName { get; set; } = null!;
    public long StockQuantity { get; set; }
    public decimal ProductPrice { get; set; }

    public string ProductDescription { get; set; } = null!;

    public bool IsOutOfStock { get; set; }

    public DateTime LastDateUpdate { get; set; }

    public int StoreId { get; set; }

    public string? ImagePath { get; set; }

    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Extra> Extras { get; set; } = new List<Extra>();

    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    public virtual Store Store { get; set; } = null!;
}
