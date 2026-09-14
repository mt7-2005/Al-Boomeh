using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class AuditTrail:BaseEntity
{
    public int Id { get; set; }

    public string EntityName { get; set; } = null!;

    public int EntityId { get; set; }

    public int AuditAction { get; set; }

    public DateTime TimestampUtc { get; set; }
    public string ChangeJson { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public virtual User User { get; set; } = null;

    
}
