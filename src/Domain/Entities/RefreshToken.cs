namespace OnlineShop.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
