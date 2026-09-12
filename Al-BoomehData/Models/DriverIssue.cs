using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class DriverIssue:BaseEntity
{
    public int Id { get; set; }

    public int DriverId { get; set; }

    public int IssueId { get; set; }

   
    public virtual Driver Driver { get; set; } = null!;

    public virtual Issue Issue { get; set; } = null!;
}
