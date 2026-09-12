using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Address:BaseEntity
{
    public int Id { get; set; }

    public string AddressName { get; set; } = null!;

    public string? Notes { get; set; }

    public int? BuildNum { get; set; }

    public string? StreetName { get; set; }

    public string Phone { get; set; } = null!;

    public string? AdditionalPhone { get; set; }

    public string Longitude { get; set; } = null!;

    public string Latitude { get; set; } = null!;

    public int? FloorNum { get; set; }

    public int? HomeNum { get; set; }

    public int CustomerId { get; set; }

   
    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
