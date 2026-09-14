using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Al_BoomehDAL.Models;

public partial class Order:BaseEntity,IAuditable
{
    public int Id { get; set; }
    public int OrderType { get; set; }
    public string? OrderCode { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }
    public int Status { get; set; }
    public decimal? TotalAmount { get; set; }

    public decimal? SubTotal { get; set; }


    public decimal? Tips { get; set; }



    public int? AddressId { get; set; }

    public string? DriverNotes { get; set; }

    public decimal? ServiceFees { get; set; }

    public decimal? DeliveryFees { get; set; }

    public string? StoreNotes { get; set; }

    public int? VoucherId { get; set; }

    public int? DriverCode { get; set; }

    public decimal? Tax { get; set; }

    public int EstimatedPreparingTime { get; set; }

    public int EstimatedDeliveryTime { get; set; }

    public string? DriverInstructions { get; set; }

    public int? DriverId { get; set; }

    public DateTime? ActualReceivingTime { get; set; }

    public int? PaymentMethod { get; set; }

   
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public double? Distance { get; set; }

    public virtual Address? Address { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Driver? Driver { get; set; }

    public virtual ICollection<OrderFeedback> OrderFeedbacks { get; set; } = new List<OrderFeedback>();

    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual Store Store { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }
}
