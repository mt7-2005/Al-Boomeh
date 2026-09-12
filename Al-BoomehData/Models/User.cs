using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;
public partial class User : BaseEntity
{
    public Guid Id { get; set; }= Guid.NewGuid();

    public UserRole Role { get; set; }
    public enum UserRole { Customer, Partner, Admin , None }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? StoreId { get; set; }
    public Store? Store { get; set; }
    public string? Password { get; set; }
    public string? PasswordSalt { get; set; }
    public string? Email { get; set; }


   
}
