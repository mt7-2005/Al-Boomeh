using Al_BoomehDAL.Models;
using Al_BoomehServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql.Internal.Postgres;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


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
            ToManyRequest,
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

            using var transaction = await
               _context.Database.BeginTransactionAsync();
            try
            {


                if (otp.AttemptsUsed >= 5) return OtpVerifyResult.ToManyRequest;

                if (otp.Code != code)
                {
                    await _context.OtpCodes
                  .Where(o => o.Id == otp.Id && o.AttemptsUsed < 5)
                  .ExecuteUpdateAsync(setters =>
                  setters.SetProperty(s => s.AttemptsUsed, s => s.AttemptsUsed + 1));
                    return OtpVerifyResult.InvalidCode;

                }

                if (otp.Code == code)
                {
                    var otpVerify = await _context.OtpCodes
                          .FromSqlInterpolated($"""
                            SELECT *
                            FROM OtpCode WITH (UPDLOCK, ROWLOCK)
                            WHERE Code = {code} AND IsUsed = 0
                        """)
                          .SingleOrDefaultAsync();
                    if(otpVerify == null) return OtpVerifyResult.InvalidCode;
                    otpVerify.IsUsed = true;

                    int rows = await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    if (rows > 0)
                    {

                        var customer = await _context.Customers
                                .FirstOrDefaultAsync(c => c.Phone == phone);
                        if (customer != null)
                        {
                            return OtpVerifyResult.Success;
                        }
                        else
                        {
                            return OtpVerifyResult.NotFoundCustomer;
                        }

                    }
                }
            }
            catch
            {
                await transaction.RollbackAsync();
            }

            return OtpVerifyResult.InvalidCode;
        }
    }
}
