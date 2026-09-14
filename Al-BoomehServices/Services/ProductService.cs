using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Al_BoomehServices.Interfaces;
using System.Threading.Tasks;

namespace Al_BoomehDAL.Classes
{
   
    public class ProductService: IProductsService
    {
        private readonly AppDbContext _context;
        private readonly IAuditScope _auditScope;

        private readonly ILogger<ProductService> _logger;
        public ProductService(AppDbContext context,ILogger<ProductService> logger, IAuditScope auditScope)
        {
            _logger = logger;
            _auditScope = auditScope;
            _context = context;
        }
        public async Task<bool> IsExist(int id)
        {
            var result = await _context.Products
                .AnyAsync(a => a.Id == id);
            return result;
        }
        public async Task<List<ProductInfoDTO>?> GetProductsByStore(int storeId, int pagenumber, int pagesize)
        {
            var productList = await _context.Products.Where(s => s.StoreId == storeId)
                                    .Select(p => new ProductInfoDTO
                                    {
                                        Id = p.Id,
                                        ProductName = p.ProductName,
                                        StockQuantity = p.StockQuantity,
                                        ProductPrice = p.ProductPrice,
                                        ProductDescription = p.ProductDescription,
                                        IsOutOfStock = p.IsOutOfStock,
                                        LastDateUpdate = p.LastDateUpdate,
                                        StoreId = p.StoreId,
                                        ImagePath = p.ImagePath,
                                        CategoryId = p.CategoryId,
                                        CreatedAt = p.CreatedAtUtc,
                                        Extras = p.Extras.Select(e => new ExtraInfoDTO
                                        {
                                            ProductId = e.ProductId,
                                            Id = e.Id,
                                            CreatedAt = e.CreatedAtUtc,
                                            ExtraName = e.ExtraName,
                                            Price = e.Price,
                                            IsMandatory = e.IsMandatory,
                                        }).ToList()
                                    }).OrderBy(p => p.ProductName)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();
            return productList;
        }
        public async Task<List<ProductInfoDTO>?> GetProductsByCategory(int categoryId, int pagenumber, int pagesize)
        {
            var productList = await _context.Products.Where(s => s.CategoryId == categoryId)
                                    .Select(p => new ProductInfoDTO
                                    {
                                        Id = p.Id,
                                        ProductName = p.ProductName,
                                        StockQuantity = p.StockQuantity,
                                        ProductPrice = p.ProductPrice,
                                        ProductDescription = p.ProductDescription,
                                        IsOutOfStock = p.IsOutOfStock,
                                        LastDateUpdate = p.LastDateUpdate,
                                        StoreId = p.StoreId,
                                        ImagePath = p.ImagePath,
                                        CategoryId = p.CategoryId,
                                        CreatedAt = p.CreatedAtUtc,
                                        Extras = p.Extras.Select(e => new ExtraInfoDTO
                                        {
                                            ProductId = e.ProductId,
                                            Id = e.Id,
                                            CreatedAt=e.CreatedAtUtc,
                                            ExtraName = e.ExtraName,
                                            Price = e.Price,
                                            IsMandatory = e.IsMandatory,
                                        }).ToList()
                                    }).OrderBy(p => p.ProductName)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();
            return productList;
        }
        public async Task<List<ProductInfoDTO>?> GetOutOfStockProducts(int pagenumber, int pagesize , int storeId)
        {
            var productList = await _context.Products.Where(s => s.IsOutOfStock == true && s.StoreId==storeId)
                                    .Select(p => new ProductInfoDTO
                                    {
                                        Id = p.Id,
                                        ProductName = p.ProductName,
                                        StockQuantity = p.StockQuantity,
                                        ProductPrice = p.ProductPrice,
                                        ProductDescription = p.ProductDescription,
                                        IsOutOfStock = p.IsOutOfStock,
                                        LastDateUpdate = p.LastDateUpdate,
                                        StoreId = p.StoreId,
                                        ImagePath = p.ImagePath,
                                        CategoryId = p.CategoryId,
                                        CreatedAt = p.CreatedAtUtc,
                                        Extras = p.Extras.Select(e => new ExtraInfoDTO
                                        {
                                            ProductId = e.ProductId,
                                            Id = e.Id,
                                            CreatedAt = e.CreatedAtUtc,
                                            ExtraName = e.ExtraName,
                                            Price = e.Price,
                                            IsMandatory = e.IsMandatory,
                                        }).ToList()
                                    }).OrderBy(p => p.ProductName)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();
            return productList;
        }
        public async Task<ProductInfoDTO?> GetProductById(int productId)
        {
            var product = await _context.Products
                               .Where(p => p.Id == productId)
                               .Select(p => new ProductInfoDTO
                               {
                                   Id = p.Id,
                                   ProductName = p.ProductName,
                                   StockQuantity = p.StockQuantity,
                                   ProductPrice = p.ProductPrice,
                                   ProductDescription = p.ProductDescription,
                                   IsOutOfStock = p.IsOutOfStock,
                                   LastDateUpdate = p.LastDateUpdate,
                                   StoreId = p.StoreId,
                                   ImagePath = p.ImagePath,
                                   CategoryId = p.CategoryId,
                                   CreatedAt = p.CreatedAtUtc,
                                   Extras = p.Extras.Select(e => new ExtraInfoDTO
                                   {
                                       ProductId = p.Id,
                                       Id = e.Id,
                                       CreatedAt = e.CreatedAtUtc,
                                       ExtraName = e.ExtraName,
                                       Price = e.Price,
                                       IsMandatory = e.IsMandatory,
                                   }).ToList()
                               }).AsNoTracking().FirstOrDefaultAsync();
            if (product == null) return null;

            return product;
        }
        public async Task<int> CreateProduct(CreateProductDTO productDTO)
        {
            if (productDTO == null) throw new NotFoundException($"failed to add product to store , no data found for update");
            if (await IsProductExistByNameInStore(productDTO.StoreId, productDTO.ProductName)) throw new ConflictException($"failed to add product to store ,product already exist");

            Product product = new Product
            {
                ProductName = productDTO.ProductName,
                StockQuantity = productDTO.StockQuantity,
                ProductPrice = productDTO.ProductPrice,
                ProductDescription = productDTO.ProductDescription,
                StoreId = productDTO.StoreId,
                ImagePath = productDTO.ImagePath,
                CategoryId = productDTO.CategoryId
            };
            await _context.AddAsync(product);
            int roweffected=await _context.SaveChangesAsync();
            if(roweffected > 0)
            {
                return product.Id;
            }
            return -1;
        }
        public async Task<bool> UpdateProduct(int productId, UpdateProductDTO productDTO)
        {
            var product =await _context.Products.Where(p => p.Id == productId).FirstOrDefaultAsync();
            if (product == null)
            {
                _logger.LogWarning("failed to update product info for id {ProductID}, no data found for update",
                   productId);
                throw new NotFoundException($"failed to update product info , no data found for update");
            }

            product.ProductName = productDTO.ProductName;
            product.StockQuantity = productDTO.StockQuantity;
            product.ProductPrice = productDTO.ProductPrice;
            product.ProductDescription = productDTO.ProductDescription;
            product.ImagePath = productDTO.ImagePath;
            product.CategoryId = productDTO.CategoryId;
            product.LastDateUpdate = DateTime.UtcNow;
            using (_auditScope.Enable())
            {
                int rowseffect = await _context.SaveChangesAsync();
                if (rowseffect > 0)
                {
                    _logger.LogInformation("update done for productId {ProductID}",
                  productId);
                    return true;
                }
            }
           
            _logger.LogWarning("failed to update product info ");
            throw new BusinessRuleException($"failed to update product info");
        }
        public async Task<bool> UpdateProductStock(int productId, UpdateProductStockDTO stockDTO)
        {
            
                var product = await _context.Products.Where(p => p.Id == productId).FirstOrDefaultAsync();
                if (product == null) return false;

                product.StockQuantity = stockDTO.StockQuantity;
                if(product.StockQuantity == 0)
                {
                 product.IsOutOfStock= true;
                }
                else
                {
                product.IsOutOfStock = stockDTO.IsOutOfStock;

                }
            product.LastDateUpdate = DateTime.UtcNow;
            using (_auditScope.Enable())
            {
                int rowseffect = await _context.SaveChangesAsync();
                return (rowseffect > 0);
            }

        }
        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
            if (product == null) return false;
            _context.Remove(product);
            using (_auditScope.Enable())
            {
                int rowseffect = await _context.SaveChangesAsync();
                return (rowseffect > 0);
            }
        }
        public async Task<Dictionary<int, int>> GetProductCountPairsCategory()
        {
            Dictionary<int, int> countpairscategory = new Dictionary<int, int>();
            var report = await _context.Products
                .GroupBy(p => p.CategoryId)
                .Select(n => new
                {
                    CategoryId = n.Key,
                    Count = n.Count()
                }).OrderBy(o => o.Count).AsNoTracking().ToListAsync();
            foreach (var row in report)
            {
                countpairscategory.Add(row.CategoryId, row.Count);
            }
            return countpairscategory;
        }
        
        public async Task<bool> IsProductExistByNameInStore(int storeId,string name)
        {
            var exist = await _context.Products
                .AnyAsync(p => p.ProductName == name&&p.StoreId==storeId);
            return exist;
        }
        public async Task<Dictionary<int,float>> TopProductPairsMonthForStore(int storeId)
        {
            Dictionary<int, float> topproduct= new Dictionary<int, float>();
            var date = DateTime.UtcNow.AddDays(-30);

            var orderIds = _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == (int)enStatus.Delivered
                         && o.CreatedAtUtc >= date
                         && o.StoreId == storeId)
                .Select(o => o.Id);

            var report = await _context.OrderLines
                .AsNoTracking()
                .Where(ol => orderIds.Contains(ol.OrderId))
                .GroupBy(p => p.ProductId)
                .Select(n => new
                {
                    ProductId = n.Key,
                    Quantity = n.Sum(c => c.Quantity),
                })
                .OrderByDescending(q => q.Quantity)
                .Take(5)
                .ToListAsync();

            foreach (var row in report)
            {
                topproduct.Add(row.ProductId, row.Quantity);
            }
                return topproduct;
        }
    
    }
}