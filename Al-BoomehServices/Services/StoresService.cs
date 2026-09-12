using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Classes
{
    public enum StoreStatusEnum
    {
        Closed,
        Open,
        Busy,
    }
    public class StoresService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StoresService> _logger;
        public StoresService(AppDbContext context,ILogger<StoresService> logger)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<bool> IsExist(int id)
        {
            var result = await _context.Stores
                .AnyAsync(a => a.Id == id);
            return result;
        }
        public async Task<List<StoreInfoDTO>?> GetAllStores(int pagenumber, int pagesize)
        {
            var storeList = await _context.Stores
                                    .Select(s => new StoreInfoDTO
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Latitude = s.Latitude,
                                        Longitude = s.Longitude,
                                        Area = s.Area,
                                        StoreStatus = (StoreStatusEnum)s.StoreStatus,
                                        Rate = s.Rate,
                                        Tax=s.Tax,
                                        CreatedAt = s.CreatedAtUtc
                                    }).OrderBy(s => s.Name)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();

            return storeList;
        }
        
        public async Task<List<StoreInfoDTO>?> GetStoresByStatus(StoreStatusEnum Status, int pagenumber, int pagesize)
        {
            var storeList = await _context.Stores.AsNoTracking().Where(s => s.StoreStatus == (int)Status)
                                    .Select(s => new StoreInfoDTO
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Latitude = s.Latitude,
                                        Longitude = s.Longitude,
                                        Area = s.Area,
                                        StoreStatus = (StoreStatusEnum)s.StoreStatus,
                                        Rate = s.Rate,
                                        Tax=s.Tax,
                                        CreatedAt = s.CreatedAtUtc
                                    }).OrderBy(s => s.Name)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .ToListAsync();
            return storeList;
        }
        public async Task<List<OrderInfoDTO>?> GetStoreOrders(int storeId, int pageNumber, int pageSize)
        {

            var orderList = await _context.Orders
                .Where(o=>o.StoreId==storeId)
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
                                CreatedAtUtc = s.CreatedAtUtc,
                                UpdatedAtUtc = s.UpdatedAtUtc,
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

        public async Task<List<StoreInfoDTO>?> GetStoresByArea(string area, int pagenumber, int pagesize)
        {
            var storeList = await _context.Stores.Where(s => s.Area == area)
                                    .Select(s => new StoreInfoDTO
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Latitude = s.Latitude,
                                        Longitude = s.Longitude,
                                        Area = s.Area,
                                        StoreStatus = (StoreStatusEnum)s.StoreStatus,
                                        Rate = s.Rate,
                                        Tax=s.Tax,
                                        CreatedAt = s.CreatedAtUtc
                                    }).OrderBy(s => s.Name)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();
            return storeList;
        }
        public async Task<StoreInfoDTO?> GetStoreById(int storeId)
        {
            var store = await _context.Stores
                               .Where(s => s.Id == storeId)
                               .Select(s => new StoreInfoDTO
                               {
                                   Id = s.Id,
                                   Name = s.Name,
                                   Latitude = s.Latitude,
                                   Longitude = s.Longitude,
                                   Area = s.Area,
                                   StoreStatus = (StoreStatusEnum)s.StoreStatus,
                                   Rate = s.Rate,
                                   Tax=s.Tax,
                                   CreatedAt = s.CreatedAtUtc
                               }).AsNoTracking().FirstOrDefaultAsync();
            if (store == null) return null;

            return store;
        }
        public async Task<List<StoreIssueDTO>?> GetStoreIssue(int storeid)
        {
            var result=await _context.StoreIssues
                .Where(s=> s.Id == storeid)
                .Select(n=> new StoreIssueDTO
                {
                   Id=n.Id,
                   StoreId= n.StoreId,
                   IssueId= n.IssueId,

                })
                .AsNoTracking().ToListAsync();
            return result;
        }
        public async Task<int> CreateStore(CreateStoreDTO storeDTO)
        {
            if (storeDTO == null) return -1;
            Store store = new Store
            {
                Name= storeDTO.Name,
                Area = storeDTO.Area,
                Rate=0,
                Latitude= storeDTO.Latitude,
                Longitude= storeDTO.Longitude,
                Tax = storeDTO.Tax,
                StoreStatus=(int)StoreStatusEnum.Open,
                CreatedAtUtc = DateTime.UtcNow
            };
            
            _context.Add(store);
            await _context.SaveChangesAsync();
            return store.Id;
        }
        public async Task<bool> UpdateStore(int storeId, UpdateStoreDTO storeDTO)
        {
            var store =await _context.Stores.Where(s => s.Id == storeId).FirstOrDefaultAsync();
            if (store == null)
            {
                _logger.LogWarning("failed to update store info for id {StoreID}, no data found for update",
                  storeId);
                throw new BusinessRuleException($"failed to update store info, no data found for update");
            }
            store.Name = storeDTO.Name;
            store.Latitude = storeDTO.Latitude;
            store.Longitude = storeDTO.Longitude;
            store.Area = storeDTO.Area;
            store.UpdatedAtUtc = DateTime.UtcNow;
            int rowseffect = await _context.SaveChangesAsync();
           if (rowseffect > 0)
            {
                _logger.LogInformation("update done for storeId {StoreID}",
                storeId);
                return true;
            }
            _logger.LogWarning("failed to update store info for id {StoreID}",
                   storeId);
            throw new BusinessRuleException($"failed to update store info ");
        }
        public async Task<bool> UpdateStoreStatus(int storeId, UpdateStoreStatusDTO statusDTO)
        {
            var store = await _context.Stores.Where(s => s.Id == storeId).FirstOrDefaultAsync();
            if (store == null) return false;
            store.StoreStatus = (int)statusDTO.StoreStatus;
            store.UpdatedAtUtc = DateTime.UtcNow;
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
        public async Task<bool> DeleteStore(int id)
        {
            var store = await _context.Stores
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();
            if (store == null) return false;
            store.IsDeleted = true;
            store.DeletedAtUtc = DateTime.UtcNow;
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
        public async Task<Dictionary<StoreStatusEnum, int>> GetStoreCountPairsStatus()
        {
            Dictionary<StoreStatusEnum, int> countpairsstatus = new Dictionary<StoreStatusEnum, int>();
            var report = await _context.Stores
                .GroupBy(s => s.StoreStatus)
                .Select(n => new
                {
                    Status = n.Key,
                    Count = n.Count()
                }).OrderBy(o => o.Count).AsNoTracking().ToListAsync();
            foreach (var row in report)
            {
                countpairsstatus.Add((StoreStatusEnum)row.Status, row.Count);
            }
            return countpairsstatus;
        }
        public async Task<bool> IsStoreExistByName(string name)
        {
            var exist = await _context.Stores
                .AnyAsync(s => s.Name == name);
            return exist;
        }
        public async Task<List<StoreIssueDTO>?> GetStoreIssues(int storeId)
        {
            var issues = await _context.StoreIssues
                .Where(s => s.StoreId == storeId)
                .Select(i => new StoreIssueDTO
                {
                    Id = i.Id,
                    StoreId = i.StoreId,
                    IssueId = i.IssueId,
                }).AsNoTracking().ToListAsync();
            return issues;
        }
       
        public async Task<StoreIssueDTO> GetStoreIssueById(int id)
        {
            var issue = await _context.StoreIssues
                .Where(i => i.Id == id)
                .Select(i => new StoreIssueDTO
                {
                    Id = i.Id,
                    StoreId = i.StoreId,
                    IssueId = i.IssueId,
                }).AsNoTracking().FirstOrDefaultAsync();
            if (issue == null) return null;
            return issue;
        }
        public async Task<int> AddIssueToStore(StoreIssueDTO issueDTO)
        {
            StoreIssue issue = new StoreIssue
            {
                StoreId = issueDTO.StoreId,
                IssueId = issueDTO.IssueId,
                CreatedAtUtc = DateTime.UtcNow,
            };
            await _context.AddAsync(issue);
            await _context.SaveChangesAsync();
            return issue.Id;
        }
        public async Task<bool> DeleteIssueFromStore(int id)
        {
            var issue = await _context.StoreIssues
                .FirstOrDefaultAsync(i => i.Id == id);
            if (issue == null) return false;
            issue.IsDeleted = true;
            issue.DeletedAtUtc = DateTime.UtcNow;
            var roweffected = await _context.SaveChangesAsync();

            return (roweffected > 0);
        }
     
    }
}