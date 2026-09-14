namespace Al_BoomehDAL.Classes
{
    public class CreateExtraDTO
    {
        public string ExtraName { get; set; } = null!;
        public bool IsMandatory { get; set; }
        public decimal? Price { get; set; }
        public int ProductId { get; set; }
    }

    public class UpdateExtraDTO
    {
        public string ExtraName { get; set; } = null!;
        public bool IsMandatory { get; set; }
        public decimal? Price { get; set; }
    }

    public class ExtraInfoDTO
    {
        public int Id { get; set; }
        public string ExtraName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public decimal? Price { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
