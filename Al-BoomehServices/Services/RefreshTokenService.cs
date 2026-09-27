using Al_BoomehDAL.Models;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Jobs;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Services
{
    public class RefreshTokenService: IRefreshTokenService, ITokenCleanup
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RefreshTokenService> _logger;
        public RefreshTokenService(AppDbContext context,ILogger<RefreshTokenService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task SaveRefreshToken(Guid userId, string rawRefreshToken, DateTime expirationDate)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                TokenHash = HashToken(rawRefreshToken),
                ExpiresAtUtc = expirationDate,
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
        public async Task TokenCleanup()
        {
            var teokenList=await _context.RefreshTokens
                .Where(r=>r.ExpiresAtUtc < DateTime.UtcNow&&r.RefreshTokenRevokedAt<=DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            if (teokenList.Any())
            {
                _context.RemoveRange(teokenList);   
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} expired refresh tokens.", teokenList.Count);
            }
            else
            {
                _logger.LogInformation("No expired refresh tokens to delete.");
            }
        }

    }
}
