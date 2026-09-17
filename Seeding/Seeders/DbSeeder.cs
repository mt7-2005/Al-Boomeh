using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Al_BoomehDAL.Models;
using Al_BoomehDAL.Data;
using Al_BoomehDAL.Seeding.Seeders;
using EFCore.BulkExtensions;

namespace Al_BoomehDAL.Seeding
{
    public class DbSeeder
    {
        private readonly AppDbContext _context;
        private readonly IAuditSuppressor _suppressor;
        private readonly Dictionary<string, (int rows, long ms)> _timings = new();

        public DbSeeder(AppDbContext context, IAuditSuppressor suppressor)
        {
            _context = context;
            _suppressor = suppressor;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Starting reseed...");
            var totalSw = Stopwatch.StartNew();

            using (_suppressor.Suppress())
            {
                await WipeAllAsync();

                await SeedCategoriesAsync();
                await SeedStoresAsync();
                await SeedProductsAsync();
                await SeedCustomersAsync();
                await SeedAddressesAsync();
                await SeedOrdersWithLinesAndHistoryAsync();
            }

            totalSw.Stop();
            PrintTimingReport(totalSw.ElapsedMilliseconds);
        }


        private async Task WipeAllAsync()
        {
            Console.WriteLine("Wiping existing data...");

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM OrderStatusHistory");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM OrderLine");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Order]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Address");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Product");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Customer");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Store");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Category");

            await ResetIdentityAsync("OrderStatusHistory");
            await ResetIdentityAsync("OrderLine");
            await ResetIdentityAsync("Order");
            await ResetIdentityAsync("Address");
            await ResetIdentityAsync("Product");
            await ResetIdentityAsync("Customer");
            await ResetIdentityAsync("Store");
            await ResetIdentityAsync("Category");

            Console.WriteLine("Wipe complete.");
        }

        private async Task ResetIdentityAsync(string tableName)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"DBCC CHECKIDENT ('{tableName}', RESEED, 0)");
        }


        private async Task SeedCategoriesAsync()
        {
            var categories = CategorySeeder.Generate();
            await SaveBatchAsync("Categories", categories);
        }

        private async Task SeedStoresAsync()
        {
            var stores = StoreSeeder.Generate(count: 50);
            await SaveBatchAsync("Stores", stores);
        }

        private async Task SeedProductsAsync()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();

            List<Store> stores = await _context.Stores.AsNoTracking()
                .ToListAsync();
            var products = ProductSeeder.Generate(categories, stores, productsPerStore: 800);
            await SaveInBatchesAsync("Products", products, batchSize: 5000);
        }

        private async Task SeedCustomersAsync()
        {
            var customers = CustomerSeeder.Generate(count: 20000);
            await SaveInBatchesAsync("Customers", customers, batchSize: 5000);
        }

        private async Task SeedAddressesAsync()
        {
            var customers = await _context.Customers.AsNoTracking().ToListAsync();

            var addresses = AddressSeeder.Generate(customers);
            await SaveInBatchesAsync("Addresses", addresses, batchSize: 5000);
        }

        private async Task SeedOrdersWithLinesAndHistoryAsync()
        {
            var customers = await _context.Customers.AsNoTracking().ToListAsync();
            var stores = await _context.Stores.AsNoTracking().ToListAsync();
            var addresses = await _context.Addresses.AsNoTracking().ToListAsync();
            var products = await _context.Products.AsNoTracking().ToListAsync();

            var (orders, lines, history) = OrderSeeder.Generate(
                customers, stores, addresses, products, orderCount: 300000);

            var orderBulkConfig = new BulkConfig
            {
                BatchSize = 20000,
                SetOutputIdentity = false,
                PreserveInsertOrder = false

            };
            var sw1 = Stopwatch.StartNew();
            await _context.BulkInsertAsync(orders, orderBulkConfig);
            sw1.Stop();
            _timings["Orders"] = (orders.Count, sw1.ElapsedMilliseconds);
            Console.WriteLine($"  Seeded {orders.Count:N0} Orders in {sw1.ElapsedMilliseconds:N0}ms");

            await SaveInBatchesAsync("OrderLines", lines, batchSize: 20000);
            await SaveInBatchesAsync("OrderStatusHistory", history, batchSize: 20000);
        }


        private async Task SaveBatchAsync<T>(string label, List<T> entities) where T : class
        {
            var sw = Stopwatch.StartNew();

            await _context.BulkInsertAsync(entities);

            sw.Stop();
            _timings[label] = (entities.Count, sw.ElapsedMilliseconds);
            Console.WriteLine($"  Seeded {entities.Count:N0} {label} in {sw.ElapsedMilliseconds:N0}ms");
        }

        private async Task SaveInBatchesAsync<T>(string label, List<T> entities, int batchSize) where T : class
        {
            var sw = Stopwatch.StartNew();

            var bulkConfig = new BulkConfig
            {
                BatchSize = batchSize,
                SetOutputIdentity = false
            };

            await _context.BulkInsertAsync(entities, bulkConfig);

            sw.Stop();
            _timings[label] = (entities.Count, sw.ElapsedMilliseconds);
            Console.WriteLine($"  Seeded {entities.Count:N0} {label} in {sw.ElapsedMilliseconds:N0}ms");
        }


        private void PrintTimingReport(long totalMs)
        {
            Console.WriteLine("\n=== Seed Timing Report ===");
            foreach (var (table, (rows, ms)) in _timings)
                Console.WriteLine($"{table,-25} {rows,10:N0} rows   {ms,8:N0} ms");
            Console.WriteLine($"\nTotal time: {totalMs / 1000.0:N1} seconds");
        }
    }
}