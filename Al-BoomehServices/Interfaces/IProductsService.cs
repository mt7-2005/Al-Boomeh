using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Al_BoomehServices.Interfaces
{
    public interface IProductsService
    {
        Task<bool> IsExist(int id);
        Task<List<ProductInfoDTO>?> GetProductsByStore(int storeId, int pagenumber, int pagesize);
        Task<List<ProductInfoDTO>?> GetProductsByCategory(int categoryId, int pagenumber, int pagesize);
        Task<List<ProductInfoDTO>?> GetOutOfStockProducts(int pagenumber, int pagesize, int storeId);
        Task<ProductInfoDTO?> GetProductById(int productId);
        Task<int> CreateProduct(CreateProductDTO productDTO);
        Task<bool> UpdateProduct(int productId, UpdateProductDTO productDTO);
        Task<bool> UpdateProductStock(int productId, UpdateProductStockDTO stockDTO);
        Task<bool> DeleteProduct(int id);
        Task<Dictionary<int, int>> GetProductCountPairsCategory();
        Task<bool> IsProductExistByNameInStore(int storeId, string name);
        Task<Dictionary<int, float>> TopProductPairsMonthForStore(int storeId);
    }
}