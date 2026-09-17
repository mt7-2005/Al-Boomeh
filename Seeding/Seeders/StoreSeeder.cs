using Bogus;
using Al_BoomehDAL.Models;

namespace Al_BoomehDAL.Seeding.Seeders
{
    public static class StoreSeeder
    {
        public static List<Store> Generate(int count = 50)
        {
           
            var faker = new Faker<Store>()
                .RuleFor(s => s.Name, f => f.Company.CompanyName())
                .RuleFor(s => s.Area, f => f.Address.City())
                .RuleFor(s => s.Latitude, f => f.Address.Latitude().ToString())
                .RuleFor(s => s.Longitude, f => f.Address.Longitude().ToString())
                .RuleFor(s => s.StoreStatus, f => 1) 
                .RuleFor(s=>s.EstimatedPreparingTime,f=>f.Random.Int(8,90))
                .RuleFor(s=>s.Tax,f => f.Random.Decimal(0, 1))
                .RuleFor(s => s.Rate, f => f.Random.Int(1, 5))
                .RuleFor(s => s.CreatedAtUtc, f => DateTime.UtcNow)
                .RuleFor(s => s.IsDeleted, f => false);
           
            return faker.Generate(count);
        }
    }
}