using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Issue:BaseEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Type { get; set; }

   

    public virtual ICollection<CustomerIssue> CustomerIssues { get; set; } = new List<CustomerIssue>();

    public virtual ICollection<DriverIssue> DriverIssues { get; set; } = new List<DriverIssue>();

    public virtual ICollection<StoreIssue> StoreIssues { get; set; } = new List<StoreIssue>();
}
