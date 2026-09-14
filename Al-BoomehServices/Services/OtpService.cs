using Al_BoomehDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Al_BoomehServices.Interfaces;


namespace Al_BoomehServices.Services
{
   
    public class OtpService: IOtpService
    {
        private readonly AppDbContext _context;
        private readonly ISmsSender _smsSender;
        public OtpService(AppDbContext context, ISmsSender smsSender)
        {
            _context = context;
            _smsSender = smsSender;
        }
        public enum OtpVerifyResult
        {
            Success,
            NotFound,
            InvalidCode,
            NotFoundCustomer
        }
        public async Task Request(string phone)
        {
            var otp=await _context.OtpCodes
                .Where(x => x.Phone == phone&&!x.IsUsed&&x.ExpiresAt>DateTime.UtcNow)
                .FirstOrDefaultAsync();
            if (otp != null)
                return;
            
            
                string code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                DateTime now= DateTime.UtcNow;
                var newOtp = new OtpCode
                {
                    Code = code,
                    Phone = phone,
                    AttemptsUsed = 0,
                    ExpiresAt = now.AddMinutes(5),
                    IsUsed = false
                };
                await _context.AddAsync(newOtp);
                int row = await _context.SaveChangesAsync();
                if (row > 0)
                {
                    await _smsSender.SendAsync(phone, $"The code was sent ");
                }
            
        }
        public async Task<OtpVerifyResult> Verify(string phone, string code)
        {
            var otp = await _context.OtpCodes
               .Where(x => x.Phone == phone && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow)
               .FirstOrDefaultAsync();

            if (otp == null) return OtpVerifyResult.NotFound;


            if (otp.AttemptsUsed >= 5) return OtpVerifyResult.InvalidCode;


            if (otp.Code == code&& otp.AttemptsUsed<5)
            {
                otp.IsUsed= true;
                await _context.SaveChangesAsync();

                var customer=await _context.Customers
                    .FirstOrDefaultAsync(c=>c.Phone == phone);
                if (customer != null)
                {
                    return OtpVerifyResult.Success;
                }
                else
                {
                    return OtpVerifyResult.NotFoundCustomer;
                }

            }
            else
            {
                otp.AttemptsUsed++;
                await _context.SaveChangesAsync();
                return OtpVerifyResult.InvalidCode;
            }
            
        }
    }
}
