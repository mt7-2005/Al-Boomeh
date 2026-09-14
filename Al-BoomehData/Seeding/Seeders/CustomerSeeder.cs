using Bogus;
using Al_BoomehDAL.Models;

namespace Al_BoomehDAL.Seeding.Seeders
{
    public static class CustomerSeeder
    {
        public static List<Customer> Generate(int count = 20000)
        {
            var faker = new Faker<Customer>()
                .RuleFor(c => c.FirstName, f => f.Name.FirstName())
                .RuleFor(c => c.LastName, f => f.Name.LastName())
                .RuleFor(c => c.Phone, (f, c) => $"079{f.Random.Int(1000000, 9999999)}")
                .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FirstName, c.LastName).ToLower())
                .RuleFor(c => c.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Past(45, DateTime.UtcNow.AddYears(-18))))
                .RuleFor(c => c.Gender, f => f.Random.Int(0, 1))       
                .RuleFor(c => c.CustomerStatus, f => f.Random.WeightedRandom(new[] { 1, 0 }, new[] { 0.95f, 0.05f })) 
                .RuleFor(c => c.ImagePath, f => null)
                .RuleFor(c => c.CreatedAtUtc, f => f.Date.Past(2))
                .RuleFor(c => c.IsDeleted, f => false);

            var customers = faker.Generate(count);

            EnsureUniqueEmails(customers);

            return customers;
        }

        private static void EnsureUniqueEmails(List<Customer> customers)
        {
            var seen = new HashSet<string>();
            var faker = new Faker();

            foreach (var customer in customers)
            {
                while (!seen.Add(customer.Email))
                {
                    customer.Email = $"{customer.Email.Split('@')[0]}{faker.Random.Int(1, 9999)}@{customer.Email.Split('@')[1]}";
                }
            }
        }
    }
}