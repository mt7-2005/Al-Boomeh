using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class StoreIssue:BaseEntity
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public int IssueId { get; set; }

  
    public virtual Issue Issue { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
