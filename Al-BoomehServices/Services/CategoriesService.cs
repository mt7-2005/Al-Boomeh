using Al_BoomehDAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Classes
{
    public class CategoriesService
    {
        private readonly AppDbContext _context;

        public CategoriesService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<CategoryInfoDTO>> GetAllCategories(int pagenumber, int pagesize)
        {
            var categoryList = await _context.Categories
                                    .Select(c => new CategoryInfoDTO
                                    {
                                        Id = c.Id,
                                        CategoryName = c.CategoryName,
                                        CreatedAt = c.CreatedAtUtc
                                    }).OrderBy(c => c.CategoryName)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();

            return categoryList;
        }
        public async Task<CategoryInfoDTO?> GetCategoryById(int categoryId)
        {
            var category = await _context.Categories
                               .Where(c => c.Id == categoryId)
                               .Select(c => new CategoryInfoDTO
                               {
                                   Id = c.Id,
                                   CategoryName = c.CategoryName,
                                   CreatedAt = c.CreatedAtUtc
                               }).AsNoTracking().FirstOrDefaultAsync();
            if (category == null) return null;

            return category;
        }
        public async Task<int> CreateCategory(CreateCategoryDTO categoryDTO)
        {
            if (categoryDTO == null) return -1;
            Category category = new Category
            {
                CategoryName = categoryDTO.CategoryName,
                CreatedAtUtc = DateTime.UtcNow

            };
            _context.Add(category);
            await _context.SaveChangesAsync();
            return category.Id;
        }
        public async Task<bool> UpdateCategory(int categoryId, UpdateCategoryDTO categoryDTO)
        {
            var category =await _context.Categories.Where(c => c.Id == categoryId).FirstOrDefaultAsync();
            if (category == null) return false;
            category.CategoryName = categoryDTO.CategoryName;
            category.UpdatedAtUtc = DateTime.UtcNow;
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
        public async Task<bool> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (category == null) return false;
            category.IsDeleted = true;
            category.DeletedAtUtc = DateTime.UtcNow;
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
      
        public async Task<bool> IsCategoryExistByName(string name)
        {
            var exist = await _context.Categories
                .AnyAsync(c => c.CategoryName == name);
            return exist;
        }
        public async Task<bool> IsExist(int id)
        {
            var result = await _context.Categories
                .AnyAsync(a => a.Id == id);
            return result;
        }
    }
}