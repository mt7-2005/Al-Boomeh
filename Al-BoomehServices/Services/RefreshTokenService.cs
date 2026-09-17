using Al_BoomehDAL.Models;
using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Al_BoomehServices.Services
{
    public class RefreshTokenService: IRefreshTokenService
    {
        private readonly AppDbContext _context;
        public RefreshTokenService(AppDbContext context)
        {
            _context = context;
        }
        public async Task SaveRefreshToken(Guid userId, string rawRefreshToken, DateTime expirationDate)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                TokenHash = HashToken(rawRefreshToken),
                ExpiresAtUtc = expirationDate,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _context.AddAsync(token);
            await _context.SaveChangesAsync();
        }


        public async Task<RefreshToken?> GetByRawToken(string rawRefreshToken)
        {
            var hash = HashToken(rawRefreshToken);
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == hash);
        }
        public async Task RevokeById(Guid tokenId)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == tokenId);
            if (token is null) return;
            token.RefreshTokenRevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllForUser(Guid userId)
        {
            try
            {
                var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RefreshTokenRevokedAt == null)
            .ToListAsync();

                if (tokens is null) return;

                foreach (var token in tokens)
                {
                    token.RefreshTokenRevokedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("Token was refresh by another request");
            }
           
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
        private static string HashToken(string token)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

    }
}
