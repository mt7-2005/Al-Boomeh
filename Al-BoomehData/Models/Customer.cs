using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Customer:BaseEntity, IAuditable
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string Email { get; set; } = null!;

    public int CustomerStatus { get; set; }

    public int Gender { get; set; }

    public string? ImagePath { get; set; }

  
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<CustomerCard> CustomerCards { get; set; } = new List<CustomerCard>();

    public virtual ICollection<CustomerIssue> CustomerIssues { get; set; } = new List<CustomerIssue>();

    public virtual ICollection<FavStore> FavStores { get; set; } = new List<FavStore>();

    public virtual ICollection<OrderFeedback> OrderFeedbacks { get; set; } = new List<OrderFeedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
