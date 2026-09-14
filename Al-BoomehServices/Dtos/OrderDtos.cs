namespace Al_BoomehDAL.Classes
{
    public class CreateOrderCartDTO
    {
        public int CustomerId { get; set; }
       
        public int StoreId { get; set; }
    
        public List<CreateOrderLineDTO> OrderLines { get; set; } = new List<CreateOrderLineDTO>();
    }

    public class PlaceOrderDTO
    {
       
        public enOrderType Type { get; set; }
     
       
        public enPaymentMethod PaymentMethod { get; set; }
        public int CustomerId { get; set; }
        public int AddressId { get; set; }
        public decimal Tips { get; set; }
       
        public string? StoreNotes { get; set; }
        public string? DriverNotes { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? VoucherCode { get; set; }

        public string? DriverInstructions { get; set; }
       
    }

    public class UpdateOrderTimeDTO
    {
        public int EstimatedPreparingTime { get; set; }
        public int EstimatedDeliveryTime { get; set; }
       
        public DateTime? ActualReceivingTime { get; set; }
    }

    public class UpdateOrderStatusDTO
    {
        public enStatus Status { get; set; }
        public int StoreId { get; set; }

    }

    public class UpdateDriverDTO
    {
        public int? DriverId { get; set; }
    }

    public class OrderInfoDTO
    {
        public int Id { get; set; }
        public int CustomerId   { get; set; }
        public string? OrderCode {  get; set; }
        public decimal? Total {  get;set; }
        public decimal? SubTotal { get;set; }
        public enPaymentMethod PaymentMethod { get; set;}
        public enStatus Status { get; set; }
        public enOrderType Type { get; set; }
        public int? DriverId { get; set; }
        public string? DriverInstructions { get;  set; }
        public string? DriverNotes { get;  set; }
        public string? StoreNotes { get;  set; }
        public decimal? Tax { get;  set; }
        public decimal? DeliveryFees { get;  set; }
        public decimal? ServiceFees { get;  set; }
        public decimal? Tips { get;  set; }
        public int? AddressId { get;  set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
       
        public int? VoucherId { get;  set; }
        public DateTime CreatedAt { get;  set; }
        public int EstimatedPreparingTime { get;  set; }
        public int? EstimatedDeliveryTime { get;  set; }
        public DateTime? ActualReceivingTime { get;  set; }
        public double? Distance {  get; set; }
        public int StoreId { get;  set; }
        public List<OrderLineDTO> OrderLines { get; set; }=new List<OrderLineDTO>();
        public List<OrderStatusHistoryDTO> OrderStatusHistory { get; set; } =new List<OrderStatusHistoryDTO>();

    }

    public class OrderStatusHistoryDTO
    {
        public enStatus OldStatus { get; set; }

        public enStatus NewStatus { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class OrderLineDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public float Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? ExtraId { get; set; }
        public decimal? ExtraPrice { get; set; }
        public string? ExtraName { get; set; }
        public string? Notes { get; set; }
        public decimal Total {  get; set; }
        public int CustomerId { get; set; }
        public int StoreId { get; set; }

    }

    public class CreateOrderLineDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public float Quantity { get; set; }
        public string? Notes { get; set; }
        public int? ExtraId { get; set; }
       

    }

    public class OrderFeedbackDTO
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public enRate Rate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
