using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Store:BaseEntity, IAuditable
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Latitude { get; set; } = null!;

    public string Longitude { get; set; } = null!;

    public string Area { get; set; } = null!;

    public int StoreStatus { get; set; }

    public int Rate { get; set; }

    public decimal? Tax { get; set; }

    public int EstimatedPreparingTime { get; set; }

    public virtual ICollection<FavStore> FavStores { get; set; } = new List<FavStore>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<StoreIssue> StoreIssues { get; set; } = new List<StoreIssue>();
}
