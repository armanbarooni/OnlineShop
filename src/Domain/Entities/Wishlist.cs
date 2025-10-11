using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class Wishlist : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid ProductId { get; private set; }
        public string? Notes { get; private set; }
        public DateTime AddedAt { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual Product Product { get; private set; } = null!;

        protected Wishlist() { }

        private Wishlist(Guid userId, Guid productId, string? notes)
        {
            UserId = userId;
            ProductId = productId;
            SetNotes(notes);
            AddedAt = DateTime.UtcNow;
            Deleted = false;
        }

        public static Wishlist Create(Guid userId, Guid productId, string? notes = null)
            => new(userId, productId, notes);

        public void SetNotes(string? notes)
        {
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string? notes, string? updatedBy)
        {
            SetNotes(notes);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این محصول قبلاً از لیست علاقه‌مندی‌ها حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
