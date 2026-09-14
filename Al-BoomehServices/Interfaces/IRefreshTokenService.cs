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
    public interface IRefreshTokenService
    {
        Task SaveRefreshToken(Guid userId, string rawRefreshToken, DateTime expirationDate);
        Task<bool> Revoked(Guid userId);
        Task<RefreshToken?> GetByRawToken(string rawRefreshToken);
        Task RevokeById(Guid tokenId);
        Task RevokeAllForUser(Guid userId);
    }
}