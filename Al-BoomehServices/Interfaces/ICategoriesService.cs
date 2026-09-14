using Al_BoomehDAL.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface ICategoriesService
    {
        Task<List<CategoryInfoDTO>> GetAllCategories(int pagenumber, int pagesize);
        Task<CategoryInfoDTO?> GetCategoryById(int categoryId);
        Task<int> CreateCategory(CreateCategoryDTO categoryDTO);
        Task<bool> UpdateCategory(int categoryId, UpdateCategoryDTO categoryDTO);
        Task<bool> DeleteCategory(int id);
        Task<bool> IsCategoryExistByName(string name);
        Task<bool> IsExist(int id);
    }
}