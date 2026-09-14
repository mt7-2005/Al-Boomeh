using Al_BoomehDAL.Data;
using Al_BoomehDAL.Models;
using Al_BoomehDAL.Seeding.Seeders;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace Al_BoomehDAL.Seeding
{
    public class DbSeeder
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<string, (int rows, long ms)> _timings = new();

        public DbSeeder(AppDbContext context)
        {
            _context = context;
        }
        
        private DataTable BuildOrderSchema()
        {
            var table = new DataTable();

            table.Columns.Add("OrderType", typeof(int));
            table.Columns.Add("OrderCode", typeof(string));
            table.Columns.Add("CustomerID", typeof(int));
            table.Columns.Add("StoreID", typeof(int));
            table.Columns.Add("Status", typeof(int));
            table.Columns.Add("TotalAmount", typeof(decimal));
            table.Columns.Add("SubTotal", typeof(decimal));
            table.Columns.Add("Tips", typeof(decimal));
            table.Columns.Add("AddressID", typeof(int));
            table.Columns.Add("ServiceFees", typeof(decimal));
            table.Columns.Add("DeliveryFees", typeof(decimal));
            table.Columns.Add("Tax", typeof(decimal));
            table.Columns.Add("EstimatedPreparingTime", typeof(int));
            table.Columns.Add("EstimatedDeliveryTime", typeof(int));
            table.Columns.Add("PaymentMethod", typeof(int));
            table.Columns.Add("Latitude", typeof(string));
            table.Columns.Add("Longitude", typeof(string));
            table.Columns.Add("Distance", typeof(double));
            table.Columns.Add("CreatedAtUtc", typeof(DateTime));
            table.Columns.Add("IsDeleted", typeof(bool));
            return table;
        }
        private DataTable ConvertOrdersToDataTable(List<Order> orders)
        {
            var table = BuildOrderSchema();

            foreach (var order in orders)
            {
                var row = table.NewRow();

                row["OrderType"] = order.OrderType;
                row["OrderCode"] = order.OrderCode ?? (object)DBNull.Value;
                row["CustomerID"] = order.CustomerId;
                row["StoreID"] = order.StoreId;
                row["Status"] = order.Status;
                row["TotalAmount"] = order.TotalAmount;
                row["SubTotal"] = order.SubTotal;
                row["Tips"] = order.Tips ?? (object)DBNull.Value;
                row["AddressID"] = order.AddressId ?? (object)DBNull.Value;
                row["ServiceFees"] = order.ServiceFees ?? (object)DBNull.Value;
                row["DeliveryFees"] = order.DeliveryFees ?? (object)DBNull.Value;
                row["Tax"] = order.Tax ?? (object)DBNull.Value;
                row["EstimatedPreparingTime"] = order.EstimatedPreparingTime;
                row["EstimatedDeliveryTime"] = order.EstimatedDeliveryTime;
                row["PaymentMethod"] = order.PaymentMethod;
                row["Latitude"] = order.Latitude ?? (object)DBNull.Value;
                row["Longitude"] = order.Longitude ?? (object)DBNull.Value;
                row["Distance"] = order.Distance ?? (object)DBNull.Value;
                row["CreatedAtUtc"] = order.CreatedAtUtc;
                row["IsDeleted"]  = false;
                table.Rows.Add(row);
            }

            return table;
        }
        private DataTable BuildCustomerSchema()
        {
            var table = new DataTable();
            table.Columns.Add("FirstName",typeof(string));
            table.Columns.Add("LastName",typeof(string));
            table.Columns.Add("Phone",typeof(string));
            table.Columns.Add("DateOfBirth", typeof(DateOnly));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("CustomerStatus", typeof(int));
            table.Columns.Add("Gender", typeof(int));
            table.Columns.Add("ImagePath", typeof(string));
            table.Columns.Add("CreatedAtUtc", typeof(DateTime));
            return table;
        }
        private DataTable ConvertCustomersToDataTable(List<Customer> customers)
        {
            var table=BuildCustomerSchema();
            foreach (var customer in customers)
            {
                var row =table.NewRow();
                row["FirstName"] = customer.FirstName;
                row["LastName"]=customer.LastName;
                row["Phone"]=customer.Phone;
                row["Email"]= customer.Email;
                row["DateOfBirth"] = customer.DateOfBirth;
                row["ImagePath"] = customer.ImagePath;
                row["Gender"] = customer.Gender;
                row["CustomerStatus"] = customer.CustomerStatus;
                row["CreatedAtUtc"] = customer.CreatedAtUtc;
                table.Rows.Add(row);
            }
            return table;
        }
        private DataTable BuildAddressSchema()
        {
            var table=new DataTable();
            table.Columns.Add("AddressName",typeof(string));
            table.Columns.Add("Notes", typeof(string));
            table.Columns.Add("StreetName", typeof(string));
            table.Columns.Add("Phone", typeof(string));
            table.Columns.Add("AdditionalPhone", typeof(string));
            table.Columns.Add("Longitude", typeof(string));
            table.Columns.Add("Latitude", typeof(string));
            table.Columns.Add("HomeNum", typeof(int));
            table.Columns.Add("FloorNum", typeof(int));
            table.Columns.Add("BuildNum", typeof(int));
            table.Columns.Add("CustomerID", typeof(int));
            table.Columns.Add("CreatedAtUtc", typeof(DateTime));
         
            return table;
        }
        private DataTable ConvertAddressesToDataTable(List<Address> addresses)
        {
            var table=BuildAddressSchema();
            foreach (var address in addresses)
            {
                var row=table.NewRow();
                row["AddressName"] = address.AddressName;
                row["Notes"] = address.Notes ?? (object)DBNull.Value;
                row["BuildNum"] = address.BuildNum ?? (object)DBNull.Value;
                row["StreetName"] = address.StreetName ?? (object)DBNull.Value;
                row["Phone"] = address.Phone;
                row["AdditionalPhone"] = address.AdditionalPhone ?? (object)DBNull.Value;
                row["Longitude"] = address.Longitude;
                row["Latitude"] = address.Latitude;
                row["FloorNum"] = address.FloorNum ??(object)DBNull.Value;
                row["HomeNum"] = address.HomeNum ?? (object)DBNull.Value;
                row["CustomerID"] = address.CustomerId;
                row["CreatedAtUtc"] = address.CreatedAtUtc;
                table.Rows.Add(row);
            }
            return table;
        }
        private DataTable BuildProductSchema()
        {
            var table=new DataTable();
            table.Columns.Add("CategoryID", typeof(int));
            table.Columns.Add("ImagePath", typeof(string));
            table.Columns.Add("StoreID", typeof(int));
            table.Columns.Add("LastDateUpdate", typeof(DateTime));
            table.Columns.Add("IsOutOfStock", typeof(bool));
            table.Columns.Add("ProductDescription", typeof(string));
            table.Columns.Add("ProductPrice", typeof(decimal));
            table.Columns.Add("StockQuantity", typeof(long));
            table.Columns.Add("ProductName", typeof(string));
            table.Columns.Add("CreatedAtUtc", typeof(DateTime));
            table.Columns.Add("IsDeleted", typeof(bool));
            return table;
        }
        private DataTable ConvertProductsToDataTable(List<Product> products)
        {
            var table = BuildProductSchema();
            foreach (var product in products)
            {
                var row = table.NewRow();
                row["ProductName"] = product.ProductName;
                row["StockQuantity"] = product.StockQuantity;
                row["ProductPrice"] = product.ProductPrice;
                row["ProductDescription"] = product.ProductDescription;
                row["IsOutOfStock"] = product.IsOutOfStock;
                row["LastDateUpdate"] = product.LastDateUpdate;
                row["StoreID"] = product.StoreId;
                row["ImagePath"] = product.ImagePath;
                row["CategoryID"] = product.CategoryId;
                row["CreatedAtUtc"] = product.CreatedAtUtc;
                row["IsDeleted"] = product.IsDeleted;
                table.Rows.Add(row);
            }
            return table;
        }
        public async Task RunAsync()
        {
            Console.WriteLine("Starting reseed...");
            var totalSw = Stopwatch.StartNew();

           
                //await WipeAllAsync();

                //await SeedCategoriesAsync();
                //await SeedStoresAsync();
                //await SeedCustomersAsync();
                await SeedUsersAsync();
                //await SeedProductsAsync();


                ////  await _context.SaveChangesAsync();
                //await SeedAddressesAsync();



                //await SeedOrdersWithLinesAndHistoryAsync();


            
            totalSw.Stop();
            PrintTimingReport(totalSw.ElapsedMilliseconds);
        }


        private async Task WipeAllAsync()
        {
            Console.WriteLine("Wiping existing data...");
            var sw = Stopwatch.StartNew();

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM OrderStatusHistory");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM OrderLine");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Order]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Address");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Product");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users");   

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

            sw.Stop();
            Console.WriteLine($"Wipe complete in {sw.ElapsedMilliseconds:N0}ms");
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
        private async Task SeedUsersAsync()
        {
            List<int> customerIds = await _context.Customers
                .AsNoTracking()
                .Select(c => c.Id)
                .ToListAsync();
            var storesList = await _context.Stores.AsNoTracking()
                .Select(c => new ValueTuple<int, string>(c.Id, c.Name))
                .ToListAsync();

            var customerUsers = UserSeeder.GenerateForCustomers(customerIds);
            var storeUsers = UserSeeder.GenerateForStores(storesList);

            var allUsers = new List<User>();
            allUsers.AddRange(customerUsers);
            allUsers.AddRange(storeUsers);

            var dataTable = ConvertUsersToDataTable(allUsers);
            var sw = Stopwatch.StartNew();

            var connectionString = _context.Database.GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "Users",
                    BulkCopyTimeout = 300,
                };

                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(dataTable);

                sw.Stop();
                _timings["Users"] = (allUsers.Count, sw.ElapsedMilliseconds);
                Console.WriteLine($"  Seeded {allUsers.Count:N0} Users in {sw.ElapsedMilliseconds:N0}ms");
            }
        }
        private async Task SeedProductsAsync()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();

            List<int> storesId = await _context.Stores.AsNoTracking()
                .Select(s => s.Id).ToListAsync();
            var products = ProductSeeder.Generate(categories, storesId, productsPerStore: 800);
            var dataTable = ConvertProductsToDataTable(products);
            var sw = Stopwatch.StartNew();

            var connectionString = _context.Database.GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "Product",
                    BulkCopyTimeout = 300,
                    //BatchSize = 100000,
                };

                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(dataTable);


                sw.Stop();
                _timings["Product"] = (products.Count, sw.ElapsedMilliseconds);
                Console.WriteLine($"  Seeded {products.Count:N0} Product in {sw.ElapsedMilliseconds:N0}ms");
            }
        }

        private async Task SeedCustomersAsync()
        {
            var customers = CustomerSeeder.Generate(count: 20000);
            var dataTable = ConvertCustomersToDataTable(customers);
            var sw = Stopwatch.StartNew();

            var connectionString = _context.Database.GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "Customer",
                    BulkCopyTimeout = 300,
                    //BatchSize = 100000,
                };

                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(dataTable);


                sw.Stop();
                _timings["Customer"] = (customers.Count, sw.ElapsedMilliseconds);
                Console.WriteLine($"  Seeded {customers.Count:N0} Customer in {sw.ElapsedMilliseconds:N0}ms");
            }
        }

        private async Task SeedAddressesAsync()
        {
            List<(int, string)> customersId = await _context.Customers.AsNoTracking().Select(c => new ValueTuple<int, string>(c.Id, c.Phone)).ToListAsync();

            var addresses = AddressSeeder.Generate(customersId);
            var dataTable = ConvertAddressesToDataTable(addresses);
            var sw = Stopwatch.StartNew();

            var connectionString = _context.Database.GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "Address",
                    BulkCopyTimeout = 300,
                    //BatchSize = 100000,
                };

                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(dataTable);


                sw.Stop();
                _timings["Address"] = (addresses.Count, sw.ElapsedMilliseconds);
                Console.WriteLine($"  Seeded {addresses.Count:N0} Address in {sw.ElapsedMilliseconds:N0}ms");
            }
        }

        private async Task SeedOrdersWithLinesAndHistoryAsync()
        {
            var customersId = await _context.Customers.AsNoTracking().Select(c=> c.Id).ToListAsync();
            var storesId = await _context.Stores.AsNoTracking().Select(c => c.Id).ToListAsync();
            var addresses = await _context.Addresses.AsNoTracking().ToListAsync();
            var products = await _context.Products.AsNoTracking().ToListAsync();

            var (orders, lines, history) = OrderSeeder.Generate(
                customersId, storesId, addresses, products, orderCount: 300000);

            await BulkInsertOrdersAsync(orders);
            await BulkInsertOrderLinesAsync(lines);
            //await SaveInBatchesAsync("Orders", orders, batchSize: 20000);
            //await SaveInBatchesAsync("OrderLines", history, batchSize: 20000);
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

            await _context.BulkInsertAsync(entities,bulkConfig);
           
            sw.Stop();
            _timings[label] = (entities.Count, sw.ElapsedMilliseconds);
            Console.WriteLine($"  Seeded {entities.Count:N0} {label} in {sw.ElapsedMilliseconds:N0}ms");
        }
       
        private async Task BulkInsertOrdersAsync(List<Order> orders)
        {
            var sw = Stopwatch.StartNew();
        
            var dataTable = ConvertOrdersToDataTable(orders);

            var connectionString = _context.Database.GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using var bulkCopy = new SqlBulkCopy(connection,SqlBulkCopyOptions.KeepIdentity, null)
                {
                    DestinationTableName = "[Order]", 
                    BulkCopyTimeout = 300,             
                    //BatchSize = 100000,
                };

                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(dataTable);

               
                sw.Stop();
                _timings["Orders"] = (orders.Count, sw.ElapsedMilliseconds);
                Console.WriteLine($"  Seeded {orders.Count:N0} Orders in {sw.ElapsedMilliseconds:N0}ms");
            }

        }
        private DataTable BuildOrderLineSchema()
        {
            var table = new DataTable();

            table.Columns.Add("ProductID", typeof(int));
            table.Columns.Add("ProductName", typeof(string));
            table.Columns.Add("Price", typeof(decimal));
            table.Columns.Add("Quantity", typeof(float));
            table.Columns.Add("Total", typeof(decimal));
            table.Columns.Add("Notes", typeof(string));
            table.Columns.Add("OrderID", typeof(int));
            table.Columns.Add("CreatedAtUtc", typeof(DateTime));
            table.Columns.Add("IsDeleted", typeof(bool));

            return table;
        }
        private DataTable ConvertOrderLinesToDataTable(List<OrderLine> lines)
        {
            var table = BuildOrderLineSchema();

            foreach (var line in lines)
            {
                var row = table.NewRow();

                row["ProductID"] = line.ProductId;
                row["ProductName"] = line.ProductName ?? (object)DBNull.Value;
                row["Price"] = line.Price;
                row["Quantity"] = line.Quantity;
                row["Total"] = line.Total;
                row["Notes"] = line.Notes ?? (object)DBNull.Value;
                row["OrderID"] = line.OrderId;
                row["CreatedAtUtc"] = line.CreatedAtUtc;
                row["IsDeleted"] = line.IsDeleted;

                table.Rows.Add(row);
            }

            return table;
        }
        private async Task BulkInsertOrderLinesAsync(List<OrderLine> lines)
        {
            var sw = Stopwatch.StartNew();

            var dataTable = ConvertOrderLinesToDataTable(lines);

            var connectionString = _context.Database.GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "OrderLine",
                    BulkCopyTimeout = 300,
                  //  BatchSize = 500000,
                };

                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(dataTable);
                //await _context.AddRangeAsync(lines);
                //await _context.SaveChangesAsync();

                sw.Stop();
                _timings["OrderLines"] = (lines.Count, sw.ElapsedMilliseconds);
                Console.WriteLine($"  Seeded {lines.Count:N0} OrderLines in {sw.ElapsedMilliseconds:N0}ms");
            }
            
           

         
        }
        private DataTable BuildUserSchema()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(Guid));
            table.Columns.Add("Role", typeof(User.UserRole));
            table.Columns.Add("CustomerID", typeof(int));
            table.Columns.Add("StoreID", typeof(int));
            table.Columns.Add("Password", typeof(string));
            table.Columns.Add("PasswordSalt", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("CreatedAtUtc", typeof(DateTime));
            table.Columns.Add("IsDeleted", typeof(bool));
            return table;
        }

        private DataTable ConvertUsersToDataTable(List<User> users)
        {
            var table = BuildUserSchema();

            foreach (var user in users)
            {
                var row = table.NewRow();

                row["Id"] = user.Id;
                row["Role"] = user.Role;
                row["CustomerID"] = user.CustomerId ?? (object)DBNull.Value;
                row["StoreID"] = user.StoreId ?? (object)DBNull.Value;
                row["Password"] = user.Password ?? (object)DBNull.Value;
                row["PasswordSalt"] = user.PasswordSalt ?? (object)DBNull.Value;
                row["Email"] = user.Email ?? (object)DBNull.Value;
                row["CreatedAtUtc"] = DateTime.UtcNow;
                row["IsDeleted"] = false;

                table.Rows.Add(row);
            }

            return table;
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