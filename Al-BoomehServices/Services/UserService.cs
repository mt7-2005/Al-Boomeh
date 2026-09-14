using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Al_BoomehDAL.Models.User;
using Al_BoomehServices.Interfaces;


namespace Al_BoomehServices.Services
{

    public class UserService: IUsersService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetUser(string email)
        {
            var user= await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);
            return user;
        }
       
        public async Task<User?> GetUser(int customerId)
        {
            var user = await _context.Users
                .Where(u => u.CustomerId == customerId)
                .FirstOrDefaultAsync();
            return user;
        }
        public async Task<User?> GetUser(Guid userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
            return user;
        }
       public async Task<User?> GetUserByPhone(string phone)
        {
            var user=await _context.Users
                .Where(u=>u.Customer.Phone == phone)
                .FirstOrDefaultAsync();
            return user;
        }
        public async Task<Guid> CreatePartner(PartnerDto partner)
        {
            if(partner==null)
                return Guid.Empty;

            byte[] salt = RandomNumberGenerator.GetBytes(16);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(partner.Password))
            {
                Salt = salt,
                DegreeOfParallelism = 4,
                MemorySize = 65536,
                Iterations = 3
            };

            byte[] hash = argon2.GetBytes(32);

            var newPartner = new User
            {
                Role = User.UserRole.Partner,
                Password = Convert.ToBase64String(hash),
                PasswordSalt = Convert.ToBase64String(salt),
                StoreId =partner.StoreId,
                Email= partner.Email,
            };
            await _context.AddAsync(newPartner);
            await _context.SaveChangesAsync();
            return newPartner.Id;
        }
    }
}
