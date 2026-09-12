namespace Al_BoomehServices.Services
{
    public class RefreshTokenDto
    {
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } 
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RefreshTokenRevokedAt { get; set; }
    }
}
