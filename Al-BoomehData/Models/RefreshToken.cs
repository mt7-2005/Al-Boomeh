using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;
public partial class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RefreshTokenRevokedAt { get; set; }
    public byte[] RowVersion { get; set; }
}
