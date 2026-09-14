using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Interfaces
{
    public interface IUsersService
    {
        Task<User?> GetUser(string email);
        Task<User?> GetUser(int customerId);
        Task<User?> GetUser(Guid userId);
        Task<User?> GetUserByPhone(string phone);
        Task<Guid> CreatePartner(PartnerDto partner);
    }
}