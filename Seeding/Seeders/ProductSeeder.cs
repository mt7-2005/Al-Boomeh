using Bogus;
using Al_BoomehDAL.Models;

namespace Al_BoomehDAL.Seeding.Seeders
{
    public static class ProductSeeder
    {
        private static readonly Dictionary<string, (decimal min, decimal max, string[] sampleNames)> CategoryPriceMap = new()
        {
            ["Electronics"] = (50, 2000, new[] { "Wireless Earbuds", "Phone Charger", "Bluetooth Speaker", "Power Bank", "USB Cable" }),
            ["Mobile Accessories"] = (5, 300, new[] { "Phone Case", "Screen Protector", "Car Mount", "Popsocket" }),
            ["Beverages"] = (1, 15, new[] { "Cola Can", "Orange Juice", "Water Bottle", "Iced Tea", "Energy Drink" }),
            ["Coffee & Tea"] = (2, 20, new[] { "Espresso", "Cappuccino", "Green Tea", "Turkish Coffee" }),
            ["Snacks"] = (1, 10, new[] { "Potato Chips", "Chocolate Bar", "Nuts Pack", "Cookies" }),
            ["Desserts"] = (2, 25, new[] { "Cheesecake Slice", "Ice Cream Cup", "Baklava", "Brownie" }),
            ["Bakery"] = (1, 15, new[] { "Croissant", "Baguette", "Muffin", "Bagel" }),
            ["Fast Food"] = (3, 30, new[] { "Cheeseburger", "Chicken Wrap", "French Fries", "Pizza Slice" }),
            ["Seafood"] = (10, 80, new[] { "Grilled Salmon", "Shrimp Plate", "Fish Fillet" }),
            ["Meat & Poultry"] = (8, 60, new[] { "Chicken Breast", "Ground Beef", "Lamb Chops" }),
            ["Vegetables & Fruits"] = (1, 12, new[] { "Tomatoes 1kg", "Bananas 1kg", "Apples 1kg", "Lettuce" }),
            ["Dairy"] = (1, 15, new[] { "Milk 1L", "Cheese Block", "Yogurt Cup", "Butter" }),
            ["Frozen Food"] = (3, 25, new[] { "Frozen Pizza", "Frozen Vegetables", "Ice Cream Tub" }),
            ["Pharmacy"] = (2, 100, new[] { "Pain Relief Tablets", "Vitamin C", "Hand Sanitizer" }),
            ["Health & Beauty"] = (3, 150, new[] { "Shampoo", "Face Cream", "Sunscreen" }),
            ["Household Supplies"] = (2, 50, new[] { "Paper Towels", "Trash Bags", "Dish Soap" }),
            ["Cleaning Supplies"] = (2, 40, new[] { "Floor Cleaner", "Glass Cleaner", "Sponges Pack" }),
            ["Baby Products"] = (3, 80, new[] { "Diapers Pack", "Baby Wipes", "Baby Formula" }),
            ["Pet Supplies"] = (3, 100, new[] { "Dog Food", "Cat Litter", "Pet Toy" }),
            ["Stationery"] = (1, 30, new[] { "Notebook", "Pen Set", "Sticky Notes" }),
            ["Books"] = (5, 60, new[] { "Novel", "Cookbook", "Notebook Journal" }),
            ["Toys"] = (5, 150, new[] { "Action Figure", "Puzzle", "Board Game" }),
            ["Sportswear"] = (10, 250, new[] { "Running Shoes", "Gym Shirt", "Yoga Mat" }),
            ["Home Decor"] = (5, 300, new[] { "Wall Clock", "Candle Set", "Photo Frame" }),
            ["Kitchenware"] = (5, 200, new[] { "Frying Pan", "Knife Set", "Mixing Bowl" }),
            ["Organic Food"] = (3, 40, new[] { "Organic Honey", "Organic Oats", "Organic Nuts" }),
            ["Fashion"] = (10, 400, new[] { "T-Shirt", "Jeans", "Jacket" }),
            ["Groceries"] = (1, 20, new[] { "Rice 5kg", "Sugar 1kg", "Flour 1kg", "Cooking Oil" }),
            ["Flowers"] = (5, 80, new[] { "Rose Bouquet", "Tulip Bunch", "Orchid Pot" }),
            ["Party Supplies"] = (2, 60, new[] { "Balloons Pack", "Party Hat", "Confetti" }),
        };

        public static List<Product> Generate(List<Category> categories, List<Store> stores, int productsPerStore = 800)
        {
            var faker = new Faker();
            var products = new List<Product>();

            foreach (var store in stores)
            {
                var usedNames = new HashSet<string>(); 

                for (int i = 0; i < productsPerStore; i++)
                {
                    var category = faker.PickRandom(categories);
                    var (min, max, sampleNames) = CategoryPriceMap.TryGetValue(category.CategoryName, out var range)
                        ? range
                        : (5m, 50m, new[] { "Generic Item" });

                    var stock = faker.Random.Int(0, 500);

                    string productName;
                    do
                    {
                        productName = $"{faker.PickRandom(sampleNames)} #{faker.Random.Int(1000, 999999)}";
                    } while (!usedNames.Add(productName));

                    products.Add(new Product
                    {
                        ProductName = productName,
                        ProductDescription = faker.Commerce.ProductDescription(),
                        ProductPrice = faker.Random.Decimal(min, max),
                        StockQuantity = stock,
                        IsOutOfStock = stock == 0,
                        LastDateUpdate = DateTime.UtcNow,
                        StoreId = store.Id,
                        CategoryId = category.Id,
                        CreatedAtUtc = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }

            return products;
        }
    }
}