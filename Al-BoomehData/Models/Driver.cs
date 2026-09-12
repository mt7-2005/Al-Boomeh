using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Driver:BaseEntity
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public int Gendor { get; set; }

    public int VehicleType { get; set; }

   

    public virtual ICollection<DriverIssue> DriverIssues { get; set; } = new List<DriverIssue>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
