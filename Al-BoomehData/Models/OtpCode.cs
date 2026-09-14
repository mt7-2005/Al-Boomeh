using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class OtpCode 
{
    public int Id { get; set; }

    public string Code { get; set; }=string.Empty;
    public bool IsUsed { get; set; }
    public string Phone {  get; set; }=string.Empty;
    public DateTime ExpiresAt {  get; set; }
    public int AttemptsUsed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
