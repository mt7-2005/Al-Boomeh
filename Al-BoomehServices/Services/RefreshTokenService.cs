using Al_BoomehDAL.Models;
using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Services
{
    public class RefreshTokenService
    {
        private readonly AppDbContext _context;
        public RefreshTokenService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveRefreshToken(Guid userId, string refreshToken, DateTime expirationDate)
        {

            var token = new RefreshToken
            {
                UserId=userId,
                TokenHash=refreshToken,
                ExpiresAtUtc=expirationDate,
                CreatedAtUtc=DateTime.UtcNow
            };

            await _context.AddAsync(token);

            await _context.SaveChangesAsync();
        }
        public async Task<RefreshTokenDto?> GetRefreshToken(Guid userId)
        {
            var refreshTokenDto=await _context.RefreshTokens
                .AsNoTracking()
                .Where(x => x.UserId == userId&&x.RefreshTokenRevokedAt==null)
                .Select(n=>new RefreshTokenDto
                {
                    UserId=n.UserId,
                    TokenHash=n.TokenHash,
                    ExpiresAtUtc=n.ExpiresAtUtc,
                    CreatedAtUtc=n.CreatedAtUtc,
                    RefreshTokenRevokedAt=n.RefreshTokenRevokedAt
                })
                .FirstOrDefaultAsync();
            return refreshTokenDto;
        }
        public async Task<bool> Revoked(Guid userId)
        {
            var refreshTokenDto = await _context.RefreshTokens
                .Where(x => x.UserId == userId && x.RefreshTokenRevokedAt == null)
                .FirstOrDefaultAsync();
            if (refreshTokenDto == null) throw new Exception("No token found");
            refreshTokenDto.RefreshTokenRevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
