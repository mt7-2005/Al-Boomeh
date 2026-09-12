using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Card:BaseEntity
{
    public int Id { get; set; }

    public string CardNumber { get; set; } = null!;

    public int Cvc { get; set; }

    public DateTime ExpirationDate { get; set; }

   

    public virtual ICollection<CustomerCard> CustomerCards { get; set; } = new List<CustomerCard>();
}
