using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Al_BoomehServices.Interfaces;


namespace Al_BoomehDAL.Classes
{
    public enum enGender
    {
        Male,
        Female
    }
    public enum enCustomerStatus
    {
        Active=1,
        Blocked=2
    }
    public class CustomerService: ICustomersService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerService> _logger;
        private readonly IAuditScope _auditScope;
        public CustomerService(AppDbContext context,ILogger<CustomerService> logger, IAuditScope auditScope)
        {
            _auditScope = auditScope;
            _logger = logger;
            _context = context;
        }
        public async Task<bool> IsExist(int id)
        {
            var result = await _context.Customers
                .AnyAsync(a => a.Id == id);
            return result;
        }
        public async Task<int> CreateCustomer(CreateCustomerDTO dto)
        {
            var customer = new Customer
            {
                ImagePath=dto.ImagePath,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                DateOfBirth = DateOnly.FromDateTime(dto.DateOfBirth),
                Gender = (int)dto.Gender,
                CustomerStatus = (int)enCustomerStatus.Active,
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return customer.Id;
        }
        public async Task<ResponseCustomerDTO?> GetCustomerById(int id)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == id)
                .AsNoTracking()
                .Select(c => new ResponseCustomerDTO
                {
                    id = c.Id,
                    ImagePath = c.ImagePath,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    DateOfBirth = c.DateOfBirth.ToDateTime(TimeOnly.MinValue),
                    Gender = (enGender)c.Gender,
                    Status=(enCustomerStatus)c.CustomerStatus,
                   
                })
                .FirstOrDefaultAsync();

            return customer;
        }
        public async Task<List<ResponseCustomerDTO>> GetAllCustomers(int pagenumber,int pagesize)
        {
            var customers = await _context.Customers
                .Select(c => new ResponseCustomerDTO
                {
                    id=c.Id,
                    ImagePath = c.ImagePath,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Status=(enCustomerStatus)c.CustomerStatus,
                    DateOfBirth = c.DateOfBirth.ToDateTime(TimeOnly.MinValue),
                    Gender = (enGender)c.Gender
                }).OrderBy(i=> i.id)
                .Skip((pagenumber - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            return customers;
        }
        public async Task<List<OrderInfoDTO>?> GetCustomerOrders(int customerId,int pageNumber,int pageSize)
        {
            var orderList = await _context.Orders
                .Where(o=> o.CustomerId==customerId)
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
                                       Distance = o.Distance,
                                       OrderLines = o.OrderLines.Select(n => new OrderLineDTO
                                       {
                                           Id = n.Id,
                                           ProductId = n.ProductId,
                                           Quantity = n.Quantity,
                                           ProductName = n.ProductName,
                                           Price = n.Price,
                                           Notes = n.Notes,
                                           Total = n.Total,
                                           ExtraId = n.ExtraId,
                                           ExtraName = n.ExtraName,
                                           ExtraPrice = n.ExtraPrice,
                                       }).ToList(),
                                       OrderStatusHistory = o.OrderStatusHistories.Select(s => new OrderStatusHistoryDTO
                                       {
                                           OldStatus = (enStatus)s.OldStatus,
                                           NewStatus = (enStatus)s.NewStatus,
                                       }).ToList()
                                   }).OrderBy(o => o.OrderCode)
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .AsNoTracking()
                                   .ToListAsync();
            return orderList;
        }
        public async Task<bool> UpdateCustomer(int id, UpdateCustomerDTO customerDTO)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (customer == null) throw new NotFoundException($"Not found customer with id {id}");
            customer.FirstName = customerDTO.FirstName;
            customer.LastName = customerDTO.LastName;
            customer.Email = customerDTO.Email;
            int rowseffect = await _context.SaveChangesAsync();
            if (rowseffect > 0)
            {
                _logger.LogInformation("update done for customerId {CustomerId}",
               id);
                return true;
            }
            _logger.LogWarning("failed to update customer info for id {CustomerID}",
                  id);
            throw new BusinessRuleException($"failed to update customer info");
        }
        public async Task<bool> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) throw new NotFoundException($"Not found customer with id {id}");

            _context.Remove(customer);

            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
        public async Task<bool> BlockCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);
            var errors = new Dictionary<string, string[]>();

            if (customer == null) throw new NotFoundException($"Not found customer with id {id}");
            if (customer.CustomerStatus == (int)enCustomerStatus.Blocked)
            {
                errors["CustomerStatus"] = ["Customer is blocked already"];
                _logger.LogWarning("failed to block customer {CustomerId}, is blocked already",
                    id);
                throw new ValidationException(errors);
            }
            customer.CustomerStatus = (int)enCustomerStatus.Blocked;

            using (_auditScope.Enable())
            {
                int rowseffect = await _context.SaveChangesAsync();
                if (rowseffect > 0)
                {
                    _logger.LogInformation("Blocked done for customerId {CustomerId}",
                     id);
                    return true;
                }
            }

            _logger.LogWarning("failed to update customer info for id {CustomerID}",
                   id);
            throw new BusinessRuleException($"failed to update customer info");
        }
        public async Task<bool> IsCustomerExist(string phone)
        {
            var result = await  _context.Customers
                .AnyAsync(c => c.Phone == phone);
            return result;
        }
        public async Task<Dictionary<enCustomerStatus,int>> GetCustomerCountByStatus()
        {
            Dictionary<enCustomerStatus,int> countStatusPairs = new Dictionary<enCustomerStatus,int>();
            var report = await _context.Customers
                .GroupBy(o => o.CustomerStatus)
                .AsNoTracking()
                .Select(n => new
                {
                    Status = n.Key,
                    Count = n.Count()
                })
                .OrderBy(o => o.Count)
                .ToListAsync();

            foreach (var row in report)
            {
                countStatusPairs.Add((enCustomerStatus)row.Status, row.Count);
            }

            return countStatusPairs;
        }
        public async Task<List<AddressDTO>?> GetCustomerAddress(int id)
        {
            var addresses = await _context.Addresses
                .Where(o=> o.CustomerId == id)
                .AsNoTracking()
                .Select(a=> new AddressDTO
                {
                    customerId=a.CustomerId,
                    Longitude = a.Longitude,
                    Latitude = a.Latitude,
                    Notes = a.Notes,
                    AdditionalPhone = a.AdditionalPhone,
                    Phone = a.Phone,
                    AddressName = a.AddressName,
                    BuildNum=a.BuildNum,
                    StreetName=a.StreetName,
                    HomeNum= a.HomeNum,
                    FloorNum= a.FloorNum,
                }).ToListAsync();

            return addresses;
        }

        public async Task<List<CardDTO>?> GetCustomerCards(int id)
        {
            var cards = await _context.CustomerCards
                .Where(c => c.CustomerId == id)
                .Select(n => new CardDTO
                {
                    CardNum = n.Card.CardNumber,
                    CVC = n.Card.Cvc,
                    ExpirationDate = n.Card.ExpirationDate,
                }).AsNoTracking() .ToListAsync();
            return cards;
        }
        public async Task<List<VoucherDTO>?> GetCustomerVouchers(int id)
        {
            var vouchers = await _context.Vouchers
                .Where(v => v.CustomerId == id)
                .Select(c => new VoucherDTO
                {
                    CustomerId = c.CustomerId,
                    MinimumDiscount = c.MinimumAmount,
                    MaximumDiscount = c.MaximumDiscount,
                    Code = c.Code,
                    IsUsed = c.IsUsed,
                    Amount = c.Amount,
                    ExpirationDate = c.ExpirationDate
                }).AsNoTracking().ToListAsync();

            return vouchers;
        }
        public async Task<List<IssueDTO>?> GetIssues(int id)
        {
            var issues= await _context.CustomerIssues
                .Where(c=> c.CustomerId == id)
                .Select(i=> new IssueDTO
                {
                    Name=i.Issue.Name,
                    IssueId = i.IssueId,
                }).AsNoTracking().ToListAsync() ;
            return issues;
        }
        public async Task<List<StoreInfoDTO>?> GetFavStores(int id)
        {
            var favstores = await _context.FavStores
                .Where(f => f.CustomerId == id)
                .Select(fs=> new StoreInfoDTO
                {
                    Id = fs.StoreId,
                    Name = fs.Store.Name,
                    StoreStatus = (StoreStatusEnum)fs.Store.StoreStatus,
                    Latitude = fs.Store.Latitude,
                    Longitude = fs.Store.Longitude,
                    Area = fs.Store.Area,
                    Rate = fs.Store.Rate,
                    CreatedAt = fs.CreatedAtUtc,

                }).AsNoTracking().ToListAsync();
            return favstores;
        }
        
        public async Task<int> AddFavStore(FavStoresDTO favStoresDTO)
        {
            if (favStoresDTO == null) return -1;
            bool exist=await _context.FavStores
                .AnyAsync(n=>n.Id == favStoresDTO.CustomerId && n.StoreId==favStoresDTO.StoreId);
            if (exist) return -1;
            FavStore store = new FavStore
            {
                CustomerId = favStoresDTO.CustomerId,
                StoreId = favStoresDTO.StoreId,
            };
            await _context.AddAsync(store);
            await _context.SaveChangesAsync();
            return store.Id;
        }
        public async Task<bool> DeleteStoreFromFav(int favstoreid)
        {
            var favstore = await _context.FavStores
                .FirstOrDefaultAsync(f=> f.Id==favstoreid);
            if (favstore == null) return false;
            _context.Remove(favstore);
            var roweffected= await _context.SaveChangesAsync();

            return (roweffected>0);

        }
        public async Task<int> AddIssueToCustomer(int customerId,int IssueId)
        {
            CustomerIssue issue = new CustomerIssue
            {
                CustomerId = customerId,
                IssueId = IssueId,
            };
            await _context.AddAsync(issue);
            await _context.SaveChangesAsync();
            return issue.CustomerId;
        }

        public async Task<bool> DeleteIssueFromCustomer(int id)
        {
            var issue=await _context.CustomerIssues
                .FirstOrDefaultAsync(i=> i.Id==id);
            if (issue == null) return false;
            issue.IsDeleted=true;
            issue.DeletedAtUtc = DateTime.UtcNow;
            var roweffected = await _context.SaveChangesAsync();

            return (roweffected > 0);
        }
        public async Task<int> AddCardToCustomer(int customerid,CardDTO cardDTO)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                Card card = new Card
                {
                    CardNumber = cardDTO.CardNum,
                    Cvc = cardDTO.CVC,
                    ExpirationDate = cardDTO.ExpirationDate,
                };

                await _context.AddAsync(card);

                CustomerCard customerCard = new CustomerCard
                {
                    CustomerId = customerid,
                    CardId = card.Id,
                };

                await _context.AddAsync(customerCard);
                await _context.SaveChangesAsync();

                transaction.Commit();
                return customerCard.Id;
            }
            catch
            {
                transaction.Rollback();
                return -1;
            }

        }
        public async Task<bool> DeleteCardFromCustomer(int customerId,int cardid)
        {
            CustomerCard? customerCard= await _context.CustomerCards
                .FirstOrDefaultAsync(c=> c.CardId==cardid&& c.CustomerId==customerId);
            if (customerCard==null) return false;

            _context.Remove(customerCard);
            int roweffected=await _context.SaveChangesAsync();
            return (roweffected > 0);
        }

    }
}
       
    
    
