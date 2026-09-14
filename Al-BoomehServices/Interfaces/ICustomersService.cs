using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface ICustomersService
    {
        Task<bool> IsExist(int id);
        Task<int> CreateCustomer(CreateCustomerDTO dto);
        Task<ResponseCustomerDTO?> GetCustomerById(int id);
        Task<List<ResponseCustomerDTO>> GetAllCustomers(int pagenumber, int pagesize);
        Task<List<OrderInfoDTO>?> GetCustomerOrders(int customerId, int pageNumber, int pageSize);
        Task<bool> UpdateCustomer(int id, UpdateCustomerDTO customerDTO);
        Task<bool> DeleteCustomer(int id);
        Task<bool> BlockCustomer(int id);
        Task<bool> IsCustomerExist(string phone);
        Task<Dictionary<enCustomerStatus, int>> GetCustomerCountByStatus();
        Task<List<AddressDTO>?> GetCustomerAddress(int id);
        Task<List<CardDTO>?> GetCustomerCards(int id);
        Task<List<VoucherDTO>?> GetCustomerVouchers(int id);
        Task<List<IssueDTO>?> GetIssues(int id);
        Task<List<StoreInfoDTO>?> GetFavStores(int id);
        Task<int> AddFavStore(FavStoresDTO favStoresDTO);
        Task<bool> DeleteStoreFromFav(int favstoreid);
        Task<int> AddIssueToCustomer(int customerId, int IssueId);
        Task<bool> DeleteIssueFromCustomer(int id);
        Task<int> AddCardToCustomer(int customerid, CardDTO cardDTO);
        Task<bool> DeleteCardFromCustomer(int customerId, int cardid);
    }
}