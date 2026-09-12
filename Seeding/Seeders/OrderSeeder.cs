using Bogus;
using Al_BoomehDAL.Models;

namespace Al_BoomehDAL.Seeding.Seeders
{
    public static class OrderSeeder
    {
        // enStatus: Holding=0, Pending=1, Accepted=2, Preparing=3, OutForDelivery=4, Delivered=5, Cancelled=6, Decline=7
        private const int Pending = 1, Accepted = 2, Preparing = 3, OutForDelivery = 4, Delivered = 5, Cancelled = 6;
        private static readonly int[] FullSuccessPath = { Pending, Accepted, Preparing, OutForDelivery, Delivered };

        public static (List<Order> orders, List<OrderLine> lines, List<OrderStatusHistory> history)
            Generate(List<Customer> customers, List<Store> stores, List<Address> addresses,
                     List<Product> products, int orderCount = 300000)
        {
            var faker = new Faker();
            var orders = new List<Order>(orderCount);
            var lines = new List<OrderLine>(orderCount * 4);
            var history = new List<OrderStatusHistory>(orderCount * 4);

            var weightedCustomerIds = BuildWeightedCustomerPool(customers, faker);

            var addressesByCustomer = addresses
                .GroupBy(a => a.CustomerId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var productsByStore = products
                .GroupBy(p => p.StoreId)
                .ToDictionary(g => g.Key, g => g.ToList());

            int orderId = 1;

            for (int i = 0; i < orderCount; i++)
            {
                var customerId = faker.PickRandom(weightedCustomerIds);

                if (!addressesByCustomer.TryGetValue(customerId, out var customerAddresses) || customerAddresses.Count == 0)
                    continue;

                var address = faker.PickRandom(customerAddresses);
                var store = faker.PickRandom(stores);

                if (!productsByStore.TryGetValue(store.Id, out var storeProducts) || storeProducts.Count == 0)
                    continue;

                var orderDate = GenerateWeightedRecentDate(faker);
                var finalStatus = PickWeightedFinalStatus(faker);

                var order = new Order
                {
                    Id = orderId,
                    OrderType = faker.Random.Int(0, 1),
                    OrderCode = $"ORD-{orderDate:yyyyMM}-{orderId:D6}",
                    CustomerId = customerId,
                    StoreId = store.Id,
                    AddressId = address.Id,
                    Status = finalStatus,
                    PaymentMethod = faker.Random.Int(0, 1),
                    EstimatedPreparingTime = faker.Random.Int(10, 30),
                    EstimatedDeliveryTime = faker.Random.Int(20, 60),
                    ServiceFees = faker.Random.Decimal(0.5m, 3m),
                    DeliveryFees = faker.Random.Decimal(1m, 5m),
                    Tax = 0,
                    Tips = faker.Random.Bool(0.3f) ? faker.Random.Decimal(0.5m, 5m) : null,
                    DriverId = null,
                    VoucherId = null,
                    Latitude = address.Latitude,
                    Longitude = address.Longitude,
                    Distance = faker.Random.Double(0.5, 15),
                    CreatedAtUtc = orderDate,
                    IsDeleted = false
                };

                int lineCount = faker.Random.Int(1, 8);
                decimal subTotal = 0;

                var chosenProducts = faker.PickRandom(storeProducts, Math.Min(lineCount, storeProducts.Count)).ToList();

                foreach (var product in chosenProducts)
                {
                    float qty = faker.Random.Int(1, 5);
                    decimal lineTotal = product.ProductPrice * (decimal)qty;
                    subTotal += lineTotal;

                    lines.Add(new OrderLine
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        Price = product.ProductPrice,
                        Quantity = qty,
                        Total = lineTotal,
                        Notes = faker.Random.Bool(0.1f) ? faker.Lorem.Sentence(3) : null,
                        CreatedAtUtc = orderDate,
                        IsDeleted = false
                    });
                }

                order.SubTotal = subTotal;
                order.TotalAmount = subTotal + order.ServiceFees.GetValueOrDefault() + order.DeliveryFees.GetValueOrDefault() + order.Tax.GetValueOrDefault();

                orders.Add(order);
                GenerateStatusHistory(order, finalStatus, orderDate, faker, history);

                orderId++;
            }

            return (orders, lines, history);
        }

        private static List<int> BuildWeightedCustomerPool(List<Customer> customers, Faker faker)
        {
            var pool = new List<int>(customers.Count * 3);

            foreach (var customer in customers)
            {
                bool isActive = faker.Random.Bool(0.2f);
                int weight = isActive ? faker.Random.Int(15, 40) : faker.Random.Int(1, 3);

                for (int i = 0; i < weight; i++)
                    pool.Add(customer.Id);
            }

            return pool;
        }

        private static DateTime GenerateWeightedRecentDate(Faker faker)
        {
            bool isRecent = faker.Random.Bool(0.6f);
            int daysAgo = isRecent ? faker.Random.Int(0, 90) : faker.Random.Int(91, 365);
            return DateTime.UtcNow.AddDays(-daysAgo).AddHours(faker.Random.Int(8, 23));
        }

        private static int PickWeightedFinalStatus(Faker faker)
        {
            return faker.Random.WeightedRandom(
                new[] { Delivered, Cancelled, Pending, Accepted, Preparing, OutForDelivery },
                new[] { 0.70f, 0.10f, 0.05f, 0.05f, 0.05f, 0.05f }
            );
        }

        private static void GenerateStatusHistory(Order order, int finalStatus, DateTime orderDate,
            Faker faker, List<OrderStatusHistory> history)
        {
            var currentTime = orderDate;
            const int Holding = 0;

            if (finalStatus == Cancelled)
            {
                bool cancelledEarly = faker.Random.Bool(0.6f);
                int cancelFromStatus = cancelledEarly ? Pending : Accepted;

                AddHistoryStep(history, order.Id, Holding, Pending, ref currentTime, faker);
                if (!cancelledEarly)
                    AddHistoryStep(history, order.Id, Pending, Accepted, ref currentTime, faker);

                AddHistoryStep(history, order.Id, cancelFromStatus, Cancelled, ref currentTime, faker);
                return;
            }

            int previousStatus = Holding;
            foreach (var step in FullSuccessPath)
            {
                AddHistoryStep(history, order.Id, previousStatus, step, ref currentTime, faker);
                previousStatus = step;

                if (step == finalStatus)
                    break;
            }
        }

        private static void AddHistoryStep(List<OrderStatusHistory> history, int orderId,
            int oldStatus, int newStatus, ref DateTime currentTime, Faker faker)
        {
            currentTime = currentTime.AddMinutes(faker.Random.Int(5, 40));

            history.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                CreatedAtUtc = currentTime,
                IsDeleted = false
            });
        }
    }
}