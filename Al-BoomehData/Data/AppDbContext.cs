using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Al_BoomehDAL.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<OtpCode> OtpCodes { get; set; }
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<AuditTrail> AuditTrails { get; set; }

    public virtual DbSet<Card> Cards { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerCard> CustomerCards { get; set; }

    public virtual DbSet<CustomerIssue> CustomerIssues { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<DriverIssue> DriverIssues { get; set; }

    public virtual DbSet<ProductOption> Extras { get; set; }

    public virtual DbSet<FavStore> FavStores { get; set; }

    public virtual DbSet<Issue> Issues { get; set; }


    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderFeedback> OrderFeedbacks { get; set; }

    public virtual DbSet<OrderLine> OrderLines { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreIssue> StoreIssues { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>().Property(x => x.RowVersion).IsRowVersion();

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Address__3214EC0753917CDC");

            entity.ToTable("Address");
            entity.HasQueryFilter(e=>!e.IsDeleted);

            entity.Property(e => e.AdditionalPhone).HasMaxLength(20);
            entity.Property(e => e.AddressName).HasMaxLength(200);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Latitude).HasMaxLength(50);
            entity.Property(e => e.Longitude).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.StreetName).HasMaxLength(200);

            entity.HasOne(d => d.Customer).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Address_Customer");
        });
        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("OtpCode");

            entity.Property(e => e.Code).IsUnicode().HasMaxLength(6);
            entity.Property(e => e.Phone).HasMaxLength(50);
          
            
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("Users");

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.Role)
                .IsRequired();

            entity.Property(e => e.CustomerId)
                .HasColumnName("CustomerID");

            entity.Property(e => e.StoreId)
                .HasColumnName("StoreID");

            entity.Property(e => e.Email)
                .HasMaxLength(255);

            entity.Property(e => e.Password)
                .HasMaxLength(500);

            entity.HasOne(e => e.Customer)
                .WithOne()
                .HasForeignKey<User>(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_User_Customer");

            entity.HasOne(e => e.Store)
                .WithMany()
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_User_Store");

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");
        });

        modelBuilder.Entity<AuditTrail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditTra__3214EC07836617FB");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.ToTable("AuditTrail");

            entity.Property(e => e.EntityName).HasMaxLength(200);
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Card__3214EC07363715A6");

            entity.ToTable("Card");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasIndex(e => e.CardNumber, "UQ__Card__A4E9FFE935DB5930").IsUnique();

            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.CreatedAtUtc).HasColumnType("datetime");
            entity.Property(e => e.Cvc).HasColumnName("CVC");
            entity.Property(e => e.DeletedAtUtc).HasColumnType("datetime");
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAtUtc).HasColumnType("datetime");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07FE4AF53B");

            entity.ToTable("Category");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07F0C14B3B");

            entity.ToTable("Customer");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasIndex(e => e.Email, "CK_Unique_CutomerEmail").IsUnique();

            entity.Property(e => e.CreatedAtUtc).HasColumnType("datetime");
            entity.Property(e => e.CustomerStatus).HasDefaultValue(1);
            entity.Property(e => e.DeletedAtUtc).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UpdatedAtUtc).HasColumnType("datetime");
        });

        modelBuilder.Entity<CustomerCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07C9921296");

            entity.HasOne(d => d.Card).WithMany(p => p.CustomerCards)
                .HasForeignKey(d => d.CardId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CustomerCards_Card");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerCards)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CustomerCards_Customer");
        });

        modelBuilder.Entity<CustomerIssue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07FCBD87FA");

            entity.ToTable("CustomerIssue");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerIssues)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CustomerIssue_Customer");

            entity.HasOne(d => d.Issue).WithMany(p => p.CustomerIssues)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CustomerIssue_Issue");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Driver__3214EC079C9B7E83");

            entity.ToTable("Driver");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<DriverIssue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DriverIs__3214EC076A7BED06");

            entity.ToTable("DriverIssue");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverIssues)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DriverIssue_Driver");

            entity.HasOne(d => d.Issue).WithMany(p => p.DriverIssues)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_DriverIssue_Issue");
        });

        modelBuilder.Entity<ProductOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Extra__3214EC0765864E9F");

            entity.ToTable("Extra");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.ExtraName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

               

            entity.HasOne(d => d.Product).WithMany(p => p.Extras)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Extra_Product");
        });

        modelBuilder.Entity<FavStore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FavStore__3214EC07DFA93B70");

            entity.ToTable("FavStore");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Customer).WithMany(p => p.FavStores)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_FavStore_Customer");

            entity.HasOne(d => d.Store).WithMany(p => p.FavStores)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_FavStore_Store");
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Issue__3214EC07FF94DAFB");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.ToTable("Issue");

            entity.Property(e => e.Name).HasMaxLength(200);
        });



        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Order");

            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC0788C73D8C");

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasIndex(e => e.OrderCode, "CK_Unique_OrderCode").IsUnique();

            entity.HasIndex(e=>e.IdempotencyKey).IsUnique();

            entity.Property(e => e.AddressId).HasColumnName("AddressID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.DeliveryFees).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DriverId).HasColumnName("DriverID");
            entity.Property(e => e.DriverInstructions).HasMaxLength(500);
            entity.Property(e => e.DriverNotes).HasMaxLength(500);
            entity.Property(e => e.OrderCode).HasMaxLength(50);
            entity.Property(e => e.ServiceFees).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.StoreNotes).HasMaxLength(500);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Tips).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VoucherId).HasColumnName("VoucherID");
            

            entity.HasOne(d => d.Address)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_Order_Address");

            

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Order_Customer");

            entity.HasOne(d => d.Driver)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.DriverId)
                .HasConstraintName("FK_Order_Driver");

            entity.HasOne(d => d.Store)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Order_Store");

            entity.HasOne(d => d.Voucher)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK_Order_Voucher");
        });

        modelBuilder.Entity<OrderFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderFee__3214EC075D421A5D");

            entity.ToTable("OrderFeedback");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderFeedbacks)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OrderFeedback_Customer");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderFeedbacks)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OrderFeedback_Order");
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderLin__3214EC0769FE7505");

            entity.ToTable("OrderLine");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e=> e.ExtraPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e=> e.Price).HasColumnType("decimal(18, 2)");
            entity.HasOne(d => d.Order).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OrderLine_Order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OrderLine_Product");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderSta__3214EC0704E77FC5");

            entity.ToTable("OrderStatusHistory");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.OrderId).HasColumnName("OrderID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_OrderStatusHistory_Order");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07B5122345");

            entity.ToTable("Product");

            entity.HasIndex(e => new { e.ProductName, e.StoreId }, "CK_Unique_ProductName").IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.ProductDescription).HasMaxLength(1000);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ProductPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Product_Category");

            entity.HasOne(d => d.Store).WithMany(p => p.Products)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Product_Store");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store__3214EC07C250506E");

            entity.ToTable("Store");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.Area).HasMaxLength(200);
            entity.Property(e => e.Latitude).HasMaxLength(50);
            entity.Property(e => e.Longitude).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.StoreStatus).HasDefaultValue(1);
        });

        modelBuilder.Entity<StoreIssue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StoreIss__3214EC078F031C22");

            entity.ToTable("StoreIssue");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(d => d.Issue).WithMany(p => p.StoreIssues)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StoreIssue_Issue");

            entity.HasOne(d => d.Store).WithMany(p => p.StoreIssues)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StoreIssue_Store");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voucher__3214EC0701EB2623");

            entity.ToTable("Voucher");
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasIndex(e => e.Code, "UQ__Voucher__A25C5AA704C29433").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.MaximumDiscount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinimumAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Vouchers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict  )
                .HasConstraintName("FK_Voucher_Customer");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
