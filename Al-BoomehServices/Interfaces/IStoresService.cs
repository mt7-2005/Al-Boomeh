using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices.Interfaces
{
    public interface IStoresService
    {
        Task<bool> IsExist(int id);
        Task<List<StoreInfoDTO>?> GetAllStores(int pagenumber, int pagesize);
        Task<List<StoreInfoDTO>?> GetStoresByStatus(StoreStatusEnum Status, int pagenumber, int pagesize);
        Task<List<OrderInfoDTO>?> GetStoreOrders(int storeId, int pageNumber, int pageSize);
        Task<List<StoreInfoDTO>?> GetStoresByArea(string area, int pagenumber, int pagesize);
        Task<StoreInfoDTO?> GetStoreById(int storeId);
        Task<List<StoreIssueDTO>?> GetStoreIssue(int storeid);
        Task<int> CreateStore(CreateStoreDTO storeDTO);
        Task<bool> UpdateStore(int storeId, UpdateStoreDTO storeDTO);
        Task<bool> UpdateStoreStatus(int storeId, UpdateStoreStatusDTO statusDTO);
        Task<bool> DeleteStore(int id);
        Task<Dictionary<StoreStatusEnum, int>> GetStoreCountPairsStatus();
        Task<bool> IsStoreExistByName(string name);
        Task<List<StoreIssueDTO>?> GetStoreIssues(int storeId);
        Task<StoreIssueDTO> GetStoreIssueById(int id);
        Task<int> AddIssueToStore(StoreIssueDTO issueDTO);
        Task<bool> DeleteIssueFromStore(int id);
    }
}