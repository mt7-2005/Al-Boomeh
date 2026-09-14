namespace Al_BoomehDAL.Classes
{
    public class CreateProductDTO
    {
        public string ProductName { get; set; } = null!;
        public long StockQuantity { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductDescription { get; set; } = null!;
        public int StoreId { get; set; }
        public string? ImagePath { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateProductDTO
    {
        public string ProductName { get; set; } = null!;
        public long StockQuantity { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductDescription { get; set; } = null!;
        public string? ImagePath { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateProductStockDTO
    {
        public int StoreId { get; set; }
        public long StockQuantity { get; set; }
        public bool IsOutOfStock { get; set; }
    }

    public class ProductInfoDTO
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public long StockQuantity { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductDescription { get; set; } = string.Empty;
        public bool IsOutOfStock { get; set; }
        public DateTime LastDateUpdate { get; set; }
        public int StoreId { get; set; }
        public string? ImagePath { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ExtraInfoDTO> Extras { get; set; } = new List<ExtraInfoDTO>();
    }
}
