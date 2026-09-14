using Bogus;
using Al_BoomehDAL.Models;

namespace Al_BoomehDAL.Seeding.Seeders
{
    public static class CategorySeeder
    {
        private static readonly string[] CategoryNames = new[]
        {
            "Fast Food", "Desserts", "Beverages", "Groceries", "Electronics",
            "Bakery", "Snacks", "Health & Beauty", "Pharmacy", "Coffee & Tea",
            "Seafood", "Meat & Poultry", "Vegetables & Fruits", "Dairy",
            "Frozen Food", "Household Supplies", "Baby Products", "Pet Supplies",
            "Stationery", "Books", "Toys", "Sportswear", "Home Decor",
            "Kitchenware", "Cleaning Supplies", "Organic Food", "Fashion",
            "Mobile Accessories", "Flowers", "Party Supplies"
        };

        public static List<Category> Generate()
        {
            var categories = CategoryNames.Select(name => new Category
            {
                CategoryName = name,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            }).ToList();

            return categories;
        }
    }
}