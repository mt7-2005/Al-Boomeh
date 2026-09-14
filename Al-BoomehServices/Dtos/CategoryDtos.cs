namespace Al_BoomehDAL.Classes
{
    public class CreateCategoryDTO
    {
        public string CategoryName { get; set; } = null!;
    }

    public class UpdateCategoryDTO
    {
        public string CategoryName { get; set; } = null!;
    }

    public class CategoryInfoDTO
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
