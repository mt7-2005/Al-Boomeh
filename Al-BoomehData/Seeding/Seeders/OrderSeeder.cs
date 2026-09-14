using Al_BoomehDAL.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Al_BoomehDAL.Seeding.Seeders
{
    public static class OrderSeeder
    {
        private const int Pending = 1, Accepted = 2, Preparing = 3, OutForDelivery = 4, Delivered = 5, Cancelled = 6;

        private static readonly int[] FullSuccessPath = { Pending, Accepted, Preparing, OutForDelivery, Delivered };
        private static HashSet<string> _ordercode = new HashSet<string>();
        private static  string _CreateOrderCode(int orderId)
        {
            var random = new Random();
            int number = random.Next(10000, 100000);
            string date = DateTime.UtcNow.ToString("yyMM");
            string count = (orderId % 1000).ToString("D3");
            string code= date + number.ToString() + count;
            if (!_ordercode.Add(code)) return _CreateOrderCode(orderId);

                return code;
        }

        public static (List<Order> orders, List<OrderLine> lines, List<OrderStatusHistory> history) 
            Generate(List<int> customersId, List<int> storesId, List<Address> addresses,
                     List<Product> products, int orderCount = 300000)
        {
            var faker = new Faker();
            var orders = new List<Order>(orderCount);
            var lines = new List<OrderLine>(orderCount * 3); 
            var history = new List<OrderStatusHistory>(orderCount * 3);

            var weightedCustomerIds = BuildWeightedCustomerPool(customersId, faker);// هاي ل كستمرز عشان تجيبهم بنسب 

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
                var store = faker.PickRandom(storesId);

                if (!productsByStore.TryGetValue(store, out var storeProducts) || storeProducts.Count == 0)
                    continue;

                var orderDate = GenerateWeightedRecentDate(faker);

                var finalStatus = PickWeightedFinalStatus(faker);

                var orderlist=new List<Order>();
                var order = new Order
                {
                   Id= orderId,
                    OrderType = faker.Random.Int(0, 1), 
                    OrderCode = _CreateOrderCode(orderId),
                    CustomerId = customerId,
                    StoreId = store,
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
                orderlist.Add(order);
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
                    });
                }

                order.SubTotal = subTotal;
                order.TotalAmount = subTotal + order.ServiceFees.GetValueOrDefault() + order.DeliveryFees.GetValueOrDefault() + order.Tax.GetValueOrDefault();

                orders.Add(order);

                GenerateStatusHistory(order.Id, finalStatus, orderDate, faker, history);

                orderId++;
            }

            return (orders, lines, history);
        }
        private static List<int> BuildWeightedCustomerPool(List<int> customers, Faker faker)
        {
            var pool = new List<int>(customers.Count * 3);

            foreach (var customer in customers)
            {
                bool isActive = faker.Random.Bool(0.2f);
                int weight = isActive ? faker.Random.Int(15, 40) : faker.Random.Int(1, 3);

                for (int i = 0; i < weight; i++)
                    pool.Add(customer);
            }

            return pool;
        }

        private static DateTime GenerateWeightedRecentDate(Faker faker)
        {
            bool isRecent = faker.Random.Bool(0.6f);
            int daysAgo = isRecent
                ? faker.Random.Int(0, 90)
                : faker.Random.Int(91, 365);

            return DateTime.UtcNow.AddDays(-daysAgo).AddHours(faker.Random.Int(1, 23));
        }

        private static int PickWeightedFinalStatus(Faker faker)
        {
            return faker.Random.WeightedRandom(
                new[] { Delivered, Cancelled, Pending, Accepted, Preparing, OutForDelivery },
                new[] { 0.70f, 0.10f, 0.05f, 0.05f, 0.05f, 0.05f }
            );
        }

        private static void GenerateStatusHistory(int orderId, int finalStatus, DateTime orderDate,
     Faker faker, List<OrderStatusHistory> history)
        {
            var currentTime = orderDate;
            const int Holding = 0;

            if (finalStatus == Cancelled)
            {
                bool cancelledEarly = faker.Random.Bool(0.6f);
                int cancelFromStatus = cancelledEarly ? Pending : Accepted;

                AddHistoryStep(history, orderId, Holding, Pending, ref currentTime, faker);
                if (!cancelledEarly)
                    AddHistoryStep(history, orderId, Pending, Accepted, ref currentTime, faker);

                AddHistoryStep(history, orderId, cancelFromStatus, Cancelled, ref currentTime, faker);
                return;
            }

            int previousStatus = Holding;
            foreach (var step in FullSuccessPath)
            {
                AddHistoryStep(history, orderId, previousStatus, step, ref currentTime, faker);
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