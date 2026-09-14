using Al_BoomehDAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;


namespace Al_BoomehDAL.Classes
{
    public class VoucherService: IVouchersService
    {
        private readonly AppDbContext _context;
        
        public VoucherService(AppDbContext context)
        {
            
            _context = context;
        }
        public async Task<int> AddVoucherToCustomer(VoucherDTO voucherDTO)
        {
            var exist=await _context.Customers
                .AnyAsync(c=> c.Id == voucherDTO.CustomerId);
            if (exist) return -1;
            Voucher voucher = new Voucher
            {
                CustomerId = voucherDTO.CustomerId,
                IsUsed = false,
                Code=voucherDTO.Code,
                Amount = voucherDTO.Amount,
                MinimumAmount = voucherDTO.MinimumDiscount,
                MaximumDiscount = voucherDTO.MaximumDiscount,
                ExpirationDate = voucherDTO.ExpirationDate,
            };
            await _context.Vouchers.AddAsync(voucher);
            await _context.SaveChangesAsync();
            return voucher.Id;
        }
        public async Task<bool> IsExist(string code)
        {
            var result = await _context.Vouchers
                .AnyAsync(a => a.Code == code);
            return result;
        }
        public async Task<bool> UseVoucher(string code)
        {
            var voucher=await _context.Vouchers
                .FirstOrDefaultAsync(voucher=> voucher.Code == code);
            if (voucher==null) return false;
            if(voucher.IsUsed) return false;
            voucher.IsUsed = true;
            voucher.CreatedAtUtc= DateTime.UtcNow;
            int roweffected = await _context.SaveChangesAsync();
            return (roweffected > 0);
           
        }
        public async Task<VoucherDTO?> GetVoucherByCode(string code)
        {
            var voucher =await _context.Vouchers.AsNoTracking()
                .FirstOrDefaultAsync(v=>  v.Code == code);
            if (voucher==null) return null;
            return new VoucherDTO
            {
                Id= voucher.Id,
                Code = code,
                MinimumDiscount = voucher.MinimumAmount,
                MaximumDiscount = voucher.MaximumDiscount,
                ExpirationDate = voucher.ExpirationDate,
                CustomerId = voucher.CustomerId,
                Amount = voucher.Amount,
                IsUsed = voucher.IsUsed,
            };
        }
    }
}
