using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class CustomerIssue:BaseEntity
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int IssueId { get; set; }

   
    public virtual Customer Customer { get; set; } = null!;

    public virtual Issue Issue { get; set; } = null!;
}
