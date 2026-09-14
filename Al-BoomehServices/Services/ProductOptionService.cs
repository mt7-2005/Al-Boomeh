using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;


namespace Al_BoomehDAL.Classes
{
    public class ProductOptionService: IExtrasService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductOptionService> _logger;
        public ProductOptionService(AppDbContext context,ILogger<ProductOptionService> logger)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<List<ExtraInfoDTO>> GetAllExtras(int pagenumber, int pagesize)
        {
            var extraList = await _context.Extras
                                    .Select(e => new ExtraInfoDTO
                                    {
                                        Id = e.Id,
                                        ExtraName = e.ExtraName,
                                        IsMandatory = e.IsMandatory,
                                        Price = e.Price,
                                        ProductId = e.ProductId,
                                        CreatedAt = e.CreatedAtUtc
                                    }).OrderBy(e => e.ExtraName)
                                    .Skip((pagenumber - 1) * pagesize)
                                    .Take(pagesize)
                                    .AsNoTracking()
                                    .ToListAsync();

            return extraList;
        }
        public async Task<bool> IsExist(int id)
        {
            var result = await _context.Extras
                .AnyAsync(a => a.Id == id);
            return result;
        }
        public async Task<List<ExtraInfoDTO>> GetExtrasByProduct(int productId)
        {
            var extraList = await _context.Extras.Where(s => s.ProductId == productId)
                                    .Select(e => new ExtraInfoDTO
                                    {
                                        Id = e.Id,
                                        ExtraName = e.ExtraName,
                                        IsMandatory = e.IsMandatory,
                                        Price = e.Price,
                                        ProductId = e.ProductId,
                                        CreatedAt = e.CreatedAtUtc
                                    }).OrderBy(e => e.ExtraName)
                                    .AsNoTracking()
                                    .ToListAsync();
            return extraList;
        }
        public async Task<List<ExtraInfoDTO>> GetMandatoryExtras(int productId)
        {
            var extraList = await _context.Extras.Where(s => s.ProductId == productId && s.IsMandatory == true)
                                    .Select(e => new ExtraInfoDTO
                                    {
                                        Id = e.Id,
                                        ExtraName = e.ExtraName,
                                        IsMandatory = e.IsMandatory,
                                        Price = e.Price,
                                        ProductId = e.ProductId,
                                        CreatedAt = e.CreatedAtUtc
                                    }).OrderBy(e => e.ExtraName)
                                    .AsNoTracking()
                                    .ToListAsync();
            return extraList;
        }
        public async Task<ExtraInfoDTO?> GetExtraById(int extraId)
        {
            var extra = await _context.Extras
                               .Where(e => e.Id == extraId)
                               .Select(e => new ExtraInfoDTO
                               {
                                   Id = e.Id,
                                   ExtraName = e.ExtraName,
                                   IsMandatory = e.IsMandatory,
                                   Price = e.Price,
                                   ProductId = e.ProductId,
                                   CreatedAt = e.CreatedAtUtc
                               }).AsNoTracking().FirstOrDefaultAsync();
            if (extra == null) return null;

            return extra;
        }
        public async Task<int> CreateExtra(CreateExtraDTO extraDto)
        {
            if (extraDto == null) return -1;
            var extra = new ProductOption
            {
                Price = extraDto.Price,
                ProductId = extraDto.ProductId,
                ExtraName = extraDto.ExtraName,
                IsMandatory = extraDto.IsMandatory,

            };
            _context.Add(extra);
            await _context.SaveChangesAsync();
            return extra.Id;
        }
        public async Task<bool> UpdateExtra(int extraId, UpdateExtraDTO extraDTO)
        {
            var extra =await _context.Extras.Where(e => e.Id == extraId).FirstOrDefaultAsync();
            if (extra == null) throw new NotFoundException($"Not found extra with id {extraId}");

            extra.ExtraName = extraDTO.ExtraName;
            extra.IsMandatory = extraDTO.IsMandatory;
            extra.Price = extraDTO.Price;
            int rowseffect = await _context.SaveChangesAsync();
            if (rowseffect > 0)
            {
                _logger.LogInformation("update done for extraId {ExtraID}",
               extraId);
                return true;
            }
            _logger.LogWarning("failed to update extra info for id {ExtraID}",
                   extraId);
            throw new BusinessRuleException($"failed to update extra info ");
        }
        public async Task<bool> DeleteExtra(int id)
        {
            var extra = await _context.Extras
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();
            if (extra == null) throw new NotFoundException($"Not found extra with id {id}");
            _context.Remove(extra);
            int rowseffect = await _context.SaveChangesAsync();
            return (rowseffect > 0);
        }
        public async Task<Dictionary<int, int>> GetExtraCountPairsProduct()
        {
            Dictionary<int, int> countpairsproduct = new Dictionary<int, int>();
            var report = await _context.Extras
                .GroupBy(e => e.ProductId)
                .Select(n => new
                {
                    ProductId = n.Key,
                    Count = n.Count()
                }).OrderBy(o => o.Count).AsNoTracking().ToListAsync();
            foreach (var row in report)
            {
                countpairsproduct.Add(row.ProductId, row.Count);
            }
            return countpairsproduct;
        }
        public async Task<bool> IsExtraExistByName(string name, int productId)
        {
            var exist = await _context.Extras
                .AnyAsync(e => e.ExtraName == name && e.ProductId == productId);
            return exist;
        }
    }
}