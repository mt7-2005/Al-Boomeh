using Al_BoomehDAL.Data;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Al_BoomehDAL.Classes
{
    public enum enOrderType
    {
        Pickup,
        Delivery
    }
    public enum enPaymentMethod
    {
        Cash,
        Credit
    }
    public enum enStatus
    {
        Holding,
        Pending,
        Accepted,
        Preparing,
        OutForDelivery,
        Delivered,
        Cancelled,
        Decline
    }
    public enum enRate
    {
        OneStar,
        TwoStar,
        ThreeStar,
        FourStar,
        FiveStar
    }
    public class OrdersService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersService> _logger;
        public OrdersService(AppDbContext context,ILogger<OrdersService> logger)
        {
            _logger = logger;
            _context = context;
        }
        private int _CreateDriverCode()
        {
            Random random = new Random();
            int number = random.Next(100, 1000);    
            return number;

        }
        public async Task<bool> IsExist(int id)
        {
            var result = await _context.Orders
                .AnyAsync(a => a.Id == id);
            return result;
        }
       
        private double _CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;

            double deltaPhi = (lat2 - lat1) * Math.PI / 180.0;
            double deltaLambda = (lon2 - lon1) * Math.PI / 180.0;

            double phi1 = lat1 * Math.PI / 180.0;
            double phi2 = lat2 * Math.PI / 180.0;

            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }
        private async Task<string> _CreateOrderCode(int orderId)
        {
            var random = new Random();
            int number = random.Next(10000, 100000);
            string date = DateTime.Now.ToString("yyMM");
            string count = (orderId % 1000).ToString("D3");
            string code = date + number.ToString() + count;
            if (await IsOrderExistByCode(code)) return await _CreateOrderCode(orderId);

            return code;
        }
        public  async Task<List<OrderInfoDTO>>GetAllOrders(int pagenumber,int pagesize)
        {
            var orderList =await _context.Orders
                                    .Select(o => new OrderInfoDTO
                                    {
                                        Id = o.Id,
                                        CustomerId = o.CustomerId,
                                        AddressId = o.AddressId,
                                       
                                        CreatedAt = o.CreatedAtUtc,
                                        DeliveryFees = o.DeliveryFees,
                                        ServiceFees = o.ServiceFees,
                                        Tips = o.Tips,
                                        DriverId = o.DriverId,
                                        DriverInstructions = o.DriverInstructions,
                                        DriverNotes = o.DriverNotes,
                                        StoreNotes = o.StoreNotes,
                                        StoreId = o.StoreId,
                                        Status = (enStatus)o.Status,
                                        Type = (enOrderType)o.OrderType,
                                        OrderCode = o.OrderCode,
                                        PaymentMethod = (enPaymentMethod)o.PaymentMethod,
                                        EstimatedDeliveryTime = o.EstimatedDeliveryTime,
                                        EstimatedPreparingTime = o.EstimatedPreparingTime,
                                        Tax = o.Tax,
                                        Total = o.TotalAmount,
                                        VoucherId = o.VoucherId,
                                        SubTotal = o.SubTotal,
                                        ActualReceivingTime = o.ActualReceivingTime,
                                        Latitude = o.Latitude,
                                        Longitude = o.Longitude,
                                        Distance=o.Distance,
                                        OrderLines=o.OrderLines.Select(n=> new OrderLineDTO
                                        {
                                            Id = n.Id,
                                            ProductId = n.ProductId,
                                            Quantity= n.Quantity,
                                            ProductName = n.ProductName,
                                            Price = n.Price,
                                            Notes = n.Notes,
                                            Total=n.Total,
                                            ExtraId=n.ExtraId,
                                            ExtraName=n.ExtraName,
                                            ExtraPrice=n.ExtraPrice,
                                        }).ToList(),
                                        OrderStatusHistory=o.OrderStatusHistories.Select(s=> new OrderStatusHistoryDTO
                                        {
                                            CreatedAtUtc = s.CreatedAtUtc,
                                            UpdatedAtUtc= s.UpdatedAtUtc,
                                            OldStatus=(enStatus)s.OldStatus,
                                            NewStatus=(enStatus)s.NewStatus,
                                        }).ToList()
                                    }).OrderBy(o=>o.OrderCode)
                                    .Skip((pagenumber-1)*pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();
           
            return orderList;
        }
        public  async Task<long> GetOrdersByStatus(enStatus Status)
        {
            var count = await _context.Orders
                               .AsNoTracking()
                               .CountAsync(o => o.Status == (int)Status);
            return count;
        }
        public  async Task<OrderInfoDTO> GetOrderById(int orderId)
        {
            var order = await _context.Orders
                               .Where(o => o.Id == orderId && o.Status!=(int)enStatus.Holding)
                               .Select(n => new OrderInfoDTO
                               {
                                   Id = n.Id,
                                   CustomerId = n.CustomerId,
                                   AddressId = n.AddressId,
                                   
                                   CreatedAt = n.CreatedAtUtc,
                                   DeliveryFees = n.DeliveryFees,
                                   ServiceFees = n.ServiceFees,
                                   Tips = n.Tips,
                                   DriverId = n.DriverId,
                                   DriverInstructions = n.DriverInstructions,
                                   DriverNotes = n.DriverNotes,
                                   StoreNotes = n.StoreNotes,
                                   StoreId = n.StoreId,
                                   Status = (enStatus)n.Status,
                                   Type = (enOrderType)n.OrderType,
                                   OrderCode = n.OrderCode,
                                   PaymentMethod = (enPaymentMethod)n.PaymentMethod,
                                   EstimatedDeliveryTime = n.EstimatedDeliveryTime,
                                   EstimatedPreparingTime = n.EstimatedPreparingTime,
                                   Tax = n.Tax,
                                   Total = n.TotalAmount,
                                   VoucherId = n.VoucherId,
                                   SubTotal = n.SubTotal,
                                   ActualReceivingTime = n.ActualReceivingTime,
                                   Latitude = n.Latitude,
                                   Longitude = n.Longitude,
                                   Distance= n.Distance,
                                   OrderStatusHistory = n.OrderStatusHistories.Select(s => new OrderStatusHistoryDTO
                                   {
                                       CreatedAtUtc = s.CreatedAtUtc,
                                       UpdatedAtUtc = s.UpdatedAtUtc,
                                       OldStatus = (enStatus)s.OldStatus,
                                       NewStatus = (enStatus)s.NewStatus,
                                   }).ToList(),
                                   OrderLines = n.OrderLines.Select(x => new OrderLineDTO
                                   {
                                       Id = x.Id,
                                       ProductId = x.ProductId,
                                       Quantity = x.Quantity,
                                       ProductName= x.ProductName,
                                       Price = x.Price,
                                       Notes = x.Notes,
                                       Total = x.Total,
                                       ExtraId = x.ExtraId,
                                       ExtraName = x.ExtraName,
                                       ExtraPrice = x.ExtraPrice,
                                   }).ToList()
                                   
                               }).AsNoTracking().FirstOrDefaultAsync();
            if (order == null) return null;

            return order;
        }
        public async Task<OrderInfoDTO> GetOrderByCode(string orderCode)
        {
            var order =await _context.Orders.Where(o=> o.OrderCode==orderCode && o.Status!=(int)enStatus.Holding)
               .Select(n => new OrderInfoDTO
               {
                   Id = n.Id,
                   CustomerId = n.CustomerId,
                   AddressId = n.AddressId,

                   CreatedAt = n.CreatedAtUtc,
                   DeliveryFees = n.DeliveryFees,
                   ServiceFees = n.ServiceFees,
                   Tips = n.Tips,
                   DriverId = n.DriverId,
                   DriverInstructions = n.DriverInstructions,
                   DriverNotes = n.DriverNotes,
                   StoreNotes = n.StoreNotes,
                   StoreId = n.StoreId,
                   Status = (enStatus)n.Status,
                   Type = (enOrderType)n.OrderType,
                   OrderCode = n.OrderCode,
                   PaymentMethod = (enPaymentMethod)n.PaymentMethod,
                   EstimatedDeliveryTime = n.EstimatedDeliveryTime,
                   EstimatedPreparingTime = n.EstimatedPreparingTime,
                   Tax = n.Tax,
                   Total = n.TotalAmount,
                   VoucherId = n.VoucherId,
                   SubTotal = n.SubTotal,
                   ActualReceivingTime = n.ActualReceivingTime,
                   Latitude = n.Latitude,
                   Longitude = n.Longitude,
                   Distance = n.Distance,
                   OrderLines = n.OrderLines.Select(x => new OrderLineDTO
                   {
                       Id = x.Id,
                       ProductId = x.ProductId,
                       Quantity = x.Quantity,
                       ProductName = x.ProductName,
                       Price = x.Price,
                       Notes = x.Notes,
                       Total = x.Total,
                       ExtraId = x.ExtraId,
                       ExtraName = x.ExtraName,
                       ExtraPrice = x.ExtraPrice,
                   }).ToList(),
                   OrderStatusHistory = n.OrderStatusHistories.Select(s => new OrderStatusHistoryDTO
                   {
                       CreatedAtUtc = s.CreatedAtUtc,
                       UpdatedAtUtc = s.UpdatedAtUtc,
                       OldStatus = (enStatus)s.OldStatus,
                       NewStatus = (enStatus)s.NewStatus,
                   }).ToList()
               }).AsSplitQuery().AsNoTracking().FirstOrDefaultAsync();
            if (order == null) return null;
            return order;
        }
        public async Task<int> CreateCart(CreateOrderCartDTO orderDTO)
        {
            using var transaction = await
    _context.Database.BeginTransactionAsync();

            if (orderDTO == null)
            {
                _logger.LogWarning("CreateCart rejected: order or line data was null");
                throw new BusinessRuleException($"CreateCart rejected: order or line data was null");
            }

            try
            {
                var order = new Order
                {
                    CustomerId= orderDTO.CustomerId,
                    Status = (int)enStatus.Holding,
                    CreatedAtUtc=DateTime.UtcNow,
                    StoreId = orderDTO.StoreId,
                };
                 await _context.AddAsync(order);
                var lineDto = orderDTO.OrderLines.First();
                var product =await _context.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p=> p.Id==lineDto.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("No product with id {ProductId}",
                        lineDto.ProductId);
                    throw new BusinessRuleException($"No product with id {lineDto.ProductId}");
                }
                var exta = lineDto.ExtraId != null ? await _context.Extras.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == lineDto.ExtraId) : null ;
                var line = new OrderLine
                {
                    ProductId = lineDto.ProductId,
                    ProductName = product.ProductName,
                    Price = product.ProductPrice,
                    ExtraId = lineDto.ExtraId != null ? lineDto.ExtraId : null,
                    ExtraName = exta != null ? exta.ExtraName : null,
                    ExtraPrice = exta != null ? exta.Price : null,
                    Quantity = lineDto.Quantity,
                    Notes = lineDto.Notes,
                    IsCart = true,
                    Order=order,
                    Total = (exta != null ? 
                    ((product.ProductPrice + exta.Price.Value) * ((decimal)lineDto.Quantity)) :
                    ((decimal)lineDto.Quantity) * product.ProductPrice),
                };
                
                await _context.AddAsync(line);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Create cart successfully with id {OrderId}",
                    order.Id);
                return order.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new BusinessRuleException("Failed to create cart");
            }

        }
        public async Task<bool> PlaceOrder(int orderId,PlaceOrderDTO orderDTO,decimal deliveryfees=0,decimal servicefees=0)
        {
            if (orderDTO == null)
            {
                _logger.LogWarning("Place order failed , empty data");
                throw new BusinessRuleException("Place order failed , empty data");
            }
            var order = await _context.Orders
                .Where(o => o.Id == orderId && o.Status == (int)enStatus.Holding)
                .FirstOrDefaultAsync();
            // var lines = order.OrderLines;
            var lines = await _context.OrderLines
                 .Where(l => l.OrderId == order.Id)
                 .Select(o => o).ToListAsync();
            if (lines == null || lines.Count == 0)
            {
                _logger.LogWarning("Place order failed , empty data");
                throw new BusinessRuleException("Place order failed , empty data");
            }
            using var transaction = await
               _context.Database.BeginTransactionAsync();

            decimal total = 0;

            
            order.SubTotal=lines.Sum(l=> l.Total);

            total += order.SubTotal.Value;

            await _context.OrderLines
                .Where(l => l.OrderId == orderId && l.IsCart)
                .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.IsCart , false));

            var store = await _context.Stores
                 .FirstOrDefaultAsync(s => s.Id == order.StoreId);
            

            try
            {
                order.OrderCode = await _CreateOrderCode(orderId);
                order.OrderType = (int)orderDTO.Type;
                foreach (var line in lines)
                {
                    var product =await _context.Products.FirstOrDefaultAsync(p => p.Id == line.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Place order failed ,product not found");
                        throw new BusinessRuleException("Place order failed ,product not found");
                    }
                    if (product.IsOutOfStock || product.StockQuantity < line.Quantity)
                    {
                        _logger.LogWarning("Place order failed ,product out of stock");
                        throw new BusinessRuleException("Place order failed ,product  out of stock");
                    }
                    product.StockQuantity-=(long)line.Quantity;
                    product.IsOutOfStock = (product.StockQuantity == 0);
                }

                order.PaymentMethod = (int)orderDTO.PaymentMethod;
                order.UpdatedAtUtc = DateTime.UtcNow;
                order.StoreNotes = orderDTO.StoreNotes;
                order.Tax = store.Tax;

                total += order.Tax.Value;

               
                order.EstimatedPreparingTime = store.EstimatedPreparingTime;
                order.Status = (int)enStatus.Pending;
                if (orderDTO.VoucherCode != null)
                {
                    var voucher = await _context.Vouchers
                        .Where(v => v.Code == orderDTO.VoucherCode).FirstOrDefaultAsync();
                    if (voucher != null&&voucher.ExpirationDate>DateTime.Now)
                    {
                        order.Voucher = voucher.IsUsed == false ? voucher : null;
                        if (!voucher.IsUsed)
                        {
                            total-=voucher.Amount;
                        }
                        voucher.IsUsed = true;
                    }
                }
                    if (order.OrderType == (int)enOrderType.Delivery)
                    {
                        order.Tips = orderDTO.Tips;
                        order.ServiceFees = servicefees * order.SubTotal;
                        total += order.Tips.Value;
                        total += order.ServiceFees.Value;
                    // * total
                        order.AddressId = orderDTO.AddressId;
                        order.Distance = _CalculateDistance(
                           Convert.ToDouble(store.Latitude), Convert.ToDouble(store.Longitude), Convert.ToDouble(orderDTO.Latitude), Convert.ToDouble(Convert.ToDouble(orderDTO.Longitude)));
                        order.DeliveryFees = (decimal)order.Distance * deliveryfees;
                        order.EstimatedDeliveryTime = (int)order.Distance * 3;
                        order.DriverInstructions = orderDTO.DriverInstructions;
                        order.DriverNotes = orderDTO.DriverNotes;
                        order.Longitude = orderDTO.Longitude;
                        order.Latitude = orderDTO.Latitude;
                    }
                order.TotalAmount=total;
                
                int roweffected= await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                if (roweffected > 0)
                {
                    _logger.LogInformation("Order Place successfully with order code {OrderCode}",
                        order.OrderCode);
                    return true;
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("Failed to place order with id {OrderId} ",
                    order.Id);
                throw new BusinessRuleException("Failed to place order");
            }
            return false;
        }
        public async Task<bool> UpdateOrderStatus(int orderId,UpdateOrderStatusDTO statusdto)
        {
            using var transaction = await
    _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders.Where(o => o.Id == orderId).FirstOrDefaultAsync();
                if (order == null)
                {
                    throw new BusinessRuleException($"Order Not Found");
                }

                var orderstatushistory = new OrderStatusHistory
                {
                    CreatedAtUtc = DateTime.UtcNow,
                    OldStatus = order.Status,
                    NewStatus = (int)statusdto.Status,
                    OrderId = orderId,
                };
                await _context.AddAsync(orderstatushistory);
                order.Status = (int)statusdto.Status;
                order.UpdatedAtUtc = DateTime.UtcNow;

                if (statusdto.Status == enStatus.Cancelled || statusdto.Status == enStatus.Decline)
                {
                    var lines=await _context.OrderLines
                        .Where(l=> l.OrderId==orderId)
                        .Select(n=> new
                        {
                            n.ProductId,
                            n.Quantity
                        }).ToListAsync();
                    var products = await _context.OrderLines
                       .Where(l => l.OrderId == orderId)
                       .Select(o => o.Product)
                       .ToListAsync();
                    
                    foreach (var product in products)
                    {
                        var line = lines.Where(l=>l.ProductId==product.Id).First();
                       
                        if (product == null) return false;
                       
                        product.StockQuantity += (int)line.Quantity;
                        product.IsOutOfStock = false;
                        product.UpdatedAtUtc = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Updating status done successfully to orderId {OrderId} new status {NewStatus}",
                            orderId, statusdto.Status);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(
                          "Order {OrderId} rejected transition to {Status} — not a valid target status",
                          orderId,statusdto.Status);
                throw new BusinessRuleException($"rejected transition to Pending — not a valid target status");
            }
            
        }
        public async Task<bool> UpdateOrderTime(int orderId,UpdateOrderTimeDTO orderTimeDTO)
        {
            var order=await _context.Orders.Where(o=> o.Id==orderId).FirstOrDefaultAsync();
            if (order == null) return false;
            order.EstimatedPreparingTime=orderTimeDTO.EstimatedPreparingTime;
            order.EstimatedDeliveryTime=orderTimeDTO.EstimatedDeliveryTime;
            order.ActualReceivingTime = orderTimeDTO.ActualReceivingTime;
            order.UpdatedAtUtc=DateTime.UtcNow;
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
        public async Task<bool> UpdateOrderDriver(int orderId,UpdateDriverDTO updateDriverDTO)
        {
            var order=await _context.Orders.Where(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order == null) return false;
            order.DriverId=updateDriverDTO.DriverId;
            order.DriverCode = _CreateDriverCode();
            order.Status = (int)enStatus.OutForDelivery;

            order.UpdatedAtUtc = DateTime.UtcNow;
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
        public async Task<bool> DeleteOrder(int id)
        {
            var order=await _context.Orders
                .Where(o=> o.Id==id)
                .FirstOrDefaultAsync();
            if (order == null) return false;
            order.IsDeleted=true;
            order.UpdatedAtUtc = DateTime.UtcNow;
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
     
        public async Task<List<OrderLineDTO>> GetOrderLines(string orderCode)
        {
            var listorderlines =await _context.OrderLines.AsNoTracking()
                .Where(o => o.Order.OrderCode == orderCode&& !o.IsCart)
                .Select(n => new OrderLineDTO
                {
                    Id = n.Id,
                    ProductId = n.ProductId,
                    Quantity = n.Quantity,
                    Price = n.Price,
                    ProductName = n.ProductName,
                    Notes = n.Notes,
                    ExtraId = n.ExtraId,
                    ExtraName = n.ExtraName,
                    ExtraPrice = n.ExtraPrice,
                }).ToListAsync();
            return listorderlines;
        }
        public async Task<int> AddOrderLine(int orderId,CreateOrderLineDTO lineDTO)
        {
            using var transaction = await
   _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == lineDTO.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("AddLineToOrder failed: product {ProductId} not found ",
                       lineDTO.ProductId);
                    throw new BusinessRuleException($"AddLineToOrder failed: product not found");

                }
                if (product.StockQuantity < (int)lineDTO.Quantity || product.IsOutOfStock)
                {
                    _logger.LogWarning("AddLineToOrder failed: product {ProductId} Out Of Stock ",
                      lineDTO.ProductId);
                    throw new BusinessRuleException($"AddLineToOrder failed: product Out Of Stock");
                }
                product.StockQuantity -= (int)lineDTO.Quantity;
                product.IsOutOfStock = (product.StockQuantity == 0);
                product.UpdatedAtUtc = DateTime.UtcNow;
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId);
                order.SubTotal += product.ProductPrice*(decimal)lineDTO.Quantity;
                order.TotalAmount+= product.ProductPrice * (decimal)lineDTO.Quantity;
                order.UpdatedAtUtc= DateTime.UtcNow;
                var extra = lineDTO.ExtraId.HasValue ? await _context.Extras
                    .Where(e=> e.Id==lineDTO.ExtraId).FirstOrDefaultAsync() : null;
                order.SubTotal += extra != null ? extra.Price.Value*(decimal)lineDTO.Quantity : 0;
                order.TotalAmount+= extra != null ? extra.Price.Value * (decimal)lineDTO.Quantity : 0;
                var line = new OrderLine
                {
                    OrderId = orderId,
                    ProductId = lineDTO.ProductId,
                    Quantity = lineDTO.Quantity,
                    ExtraId = lineDTO.ExtraId,
                    Notes = lineDTO.Notes,
                    ProductName = product.ProductName,
                    Price = product.ProductPrice,
                    ExtraName = extra != null ? extra.ExtraName : null,
                    ExtraPrice = extra != null ? extra.Price : null,
                    Total = extra != null ? (decimal)lineDTO.Quantity * (product.ProductPrice + extra.Price.Value) : (decimal)lineDTO.Quantity * (product.ProductPrice)
                };
                _context.OrderLines.Add(line);
                int roweffected= await _context.SaveChangesAsync();
                if (roweffected > 0)
                {
                    _logger.LogInformation("Line created successfully with Id {LineId}",
                line.Id);
                }

                await transaction.CommitAsync();
                return line.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("AddLineToOrder failed ");
                throw new BusinessRuleException($"AddLineToOrder failed");
            }
        }
        public async Task<int> AddLineToCart(int orderId,CreateOrderLineDTO lineDTO)
        {
            var extra = lineDTO.ExtraId.HasValue ? await _context.Extras
                    .Where(e => e.Id == lineDTO.ExtraId).FirstOrDefaultAsync() : null;
            var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == lineDTO.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Failed to add line , product not found");
                throw new BusinessRuleException("Failed to add line , product not found");
            }
            var line = new OrderLine
            {
                OrderId = orderId,
                ProductId = lineDTO.ProductId,
                Quantity = lineDTO.Quantity,
                ExtraId = lineDTO.ExtraId,
                Notes = lineDTO.Notes,
                ProductName = product.ProductName,
                Price = product.ProductPrice,
                IsCart =true,
                ExtraName = extra != null ? extra.ExtraName : null,
                ExtraPrice = extra != null ? extra.Price : null,
                Total = extra != null ? (decimal)lineDTO.Quantity * (product.ProductPrice + extra.Price.Value) : (decimal)lineDTO.Quantity * (product.ProductPrice)
            };
            _context.OrderLines.Add(line);
            int roweffected = await _context.SaveChangesAsync();
            if (roweffected > 0)
            {
                _logger.LogInformation("Line created successfully with Id {LineId}",
            line.Id);
                return line.Id;
            }
            throw new BusinessRuleException("Failed to add line");
        } 
       
        public async Task<bool> DeleteOrderLine(int Id)
        {
            using var transaction = await
   _context.Database.BeginTransactionAsync();
            try
            {
                var line = await _context.OrderLines
                    .Where(o => o.Id == Id).FirstOrDefaultAsync();
                if (line == null) return false;

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == line.OrderId);

                if (order == null) return false;
                order.SubTotal-=line.Total;
                order.TotalAmount-=line.Total;
                order.UpdatedAtUtc= DateTime.UtcNow;

                var product = await _context.Products
                    .FirstOrDefaultAsync(o => o.Id == line.ProductId);
                if (product == null) return false;
                product.StockQuantity +=(int)line.Quantity;
                product.IsOutOfStock = false;
                product.UpdatedAtUtc = DateTime.UtcNow;

                line.IsDeleted = true;
                line.UpdatedAtUtc = DateTime.UtcNow;
                int rowseffect = await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return (rowseffect > 0);
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<bool> DeleteLineFromCart(int id)
        {
            var line=await _context.OrderLines
                .Where(o=> o.Id == id&&o.IsCart).FirstOrDefaultAsync();
            if (line == null) return false;
            line.IsDeleted = true;
            line.UpdatedAtUtc = DateTime.UtcNow;
            line.DeletedAtUtc= DateTime.UtcNow;
            int roweffected=await _context.SaveChangesAsync();
            return (roweffected > 0);
        }
        public async Task<Dictionary<enStatus,int>> GetOrderCountPairsStatus()
        {
            Dictionary<enStatus,int> countpairsstatus=new Dictionary<enStatus,int>();
            var report = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(n => new
                {
                    Status = n.Key,
                    Count = n.Count()
                }).OrderBy(o => o.Count).AsNoTracking().ToListAsync();
            foreach (var row in report)
            {
                countpairsstatus.Add((enStatus)row.Status, row.Count);
            }
            return countpairsstatus;
        }
        public async Task<long> GetOrderCount()
        {
            var result = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.CreatedAtUtc >= DateTime.UtcNow.Date&&o.CreatedAtUtc<DateTime.UtcNow.AddDays(1));
            return result;
                
        }

        public async Task<int> CreateOrderFeedback( OrderFeedbackDTO feedbackDTO)
        {
            OrderFeedback feedback = new OrderFeedback
            {
                Rate = (int)feedbackDTO.Rate,
                OrderId = feedbackDTO.OrderId,
                CustomerId = feedbackDTO.CustomerId,
                Notes=feedbackDTO.Notes,
            };
            _context.Add(feedback);
           await _context.SaveChangesAsync();
            return feedback.Id;

        }
        public async Task<OrderFeedbackDTO> GetOrderFeedback(int orderid)
        {
            var feedback=await _context.OrderFeedbacks
                .Where(o=> o.OrderId == orderid)
                .Select(f=> new OrderFeedbackDTO
                {
                    Rate=(enRate)f.Rate,
                    OrderId=orderid,
                    CustomerId=f.CustomerId,
                    Notes=f.Notes,
                }).AsNoTracking().FirstOrDefaultAsync();
            if (feedback == null) return null;
           return feedback;
        }
        public async Task<bool> IsOrderExistByCode(string code)
        {
            var exist=await _context.Orders
                .AnyAsync(o=>o.OrderCode==code);
            return exist;
        }
        public async Task<decimal> AvgOrderValueLastMonth()
        {
            decimal result = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == (int)enStatus.Delivered&& o.CreatedAtUtc>=DateTime.UtcNow.AddDays(-30))
                .AverageAsync(n => n.TotalAmount.Value);
            return result;
                
        }
        public async Task<List<OrderStatusHistoryDTO>?> GetOrderStatusHistories(int orderid)
        {
            var historylist = await _context.OrderStatusHistories
                .Where(o => o.OrderId == orderid)
                .Select(n => new OrderStatusHistoryDTO
                {
                    OldStatus = (enStatus)n.OldStatus,
                    NewStatus = (enStatus)n.NewStatus,
                    CreatedAtUtc = n.CreatedAtUtc
                }).AsNoTracking()
                .ToListAsync();
            return historylist;
        }
        public async Task<OrderLineDTO?> GetOrderLine(int id)
        {
            var orderline = await _context.OrderLines
                .Where(o => o.Id == id)
                .Select(n => new OrderLineDTO
                {
                    Price = n.Price,
                    ProductId = n.ProductId,
                    Total = n.Total,
                    Quantity = n.Quantity,
                    ExtraId = n.ExtraId,
                    ExtraName = n.ExtraName,
                    ExtraPrice = n.ExtraPrice,
                    ProductName = n.ProductName,
                    Notes = n.Notes,
                    Id = n.Id,
                }).AsNoTracking()
                .FirstOrDefaultAsync();
            return orderline;
        }
        public async Task<bool> IsOrderLineExist(int id)
        {
            var result=await _context.OrderLines
                .AnyAsync(o=>o.Id == id);
            return result;
        }
    }
}
