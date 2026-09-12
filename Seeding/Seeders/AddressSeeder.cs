using Bogus;
using Al_BoomehDAL.Models;

namespace Al_BoomehDAL.Seeding.Seeders
{
    public static class AddressSeeder
    {
        private static readonly string[] AddressLabels = { "Home", "Work", "Parents House", "Apartment", "Office" };

        public static List<Address> Generate(List<Customer> customers)
        {
            var faker = new Faker();
            var addresses = new List<Address>();

            foreach (var customer in customers)
            {
                int addressCount = faker.Random.WeightedRandom(
                    new[] { 0, 1, 2, 3 },
                    new[] { 0.05f, 0.55f, 0.30f, 0.10f }
                );

                for (int i = 0; i < addressCount; i++)
                {
                    addresses.Add(new Address
                    {
                        AddressName = faker.PickRandom(AddressLabels),
                        Notes = faker.Random.Bool(0.3f) ? faker.Lorem.Sentence(5) : null,
                        BuildNum = faker.Random.Int(1, 200),
                        StreetName = faker.Address.StreetName(),
                        Phone = customer.Phone,
                        AdditionalPhone = faker.Random.Bool(0.2f) ? $"077{faker.Random.Int(1000000, 9999999)}" : null,
                        Longitude = faker.Address.Longitude().ToString(),
                        Latitude = faker.Address.Latitude().ToString(),
                        FloorNum = faker.Random.Bool(0.6f) ? faker.Random.Int(0, 15) : null,
                        HomeNum = faker.Random.Bool(0.5f) ? faker.Random.Int(1, 50) : null,
                        CustomerId = customer.Id,
                        CreatedAtUtc = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }

            return addresses;
        }
    }
}