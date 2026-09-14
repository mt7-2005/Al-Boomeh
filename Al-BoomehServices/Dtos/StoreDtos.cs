namespace Al_BoomehDAL.Classes
{
    public class CreateStoreDTO
    {
        public string Name { get; set; } = null!;
        public string Latitude { get; set; } = null!;
        public string Longitude { get; set; } = null!;
        public string Area { get; set; } = null!;
        public decimal? Tax { get; set; } 
        public StoreStatusEnum StoreStatus { get; set; }
    }

    public class UpdateStoreDTO
    {
        public string Name { get; set; } = null!;
        public string Latitude { get; set; } = null!;
        public string Longitude { get; set; } = null!;
        public string Area { get; set; } = null!;
        public decimal? Tax { get; set; } 

    }

    public class UpdateStoreStatusDTO
    {
        public StoreStatusEnum StoreStatus { get; set; }
    }

    public class StoreIssueDTO
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int IssueId { get; set; }
    }

    public class StoreInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public StoreStatusEnum StoreStatus { get; set; }
        public int Rate { get; set; }
        public decimal? Tax { get; set; } 

        public DateTime CreatedAt { get; set; }
    }
}
