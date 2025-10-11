using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class SavedCart : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid CartId { get; private set; }
        public string SavedCartName { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsFavorite { get; private set; }
        public DateTime SavedAt { get; private set; }
        public DateTime? LastAccessedAt { get; private set; }
        public int AccessCount { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual Cart Cart { get; private set; } = null!;

        protected SavedCart() { }

        private SavedCart(Guid userId, Guid cartId, string savedCartName, string? description, bool isFavorite)
        {
            UserId = userId;
            CartId = cartId;
            SetSavedCartName(savedCartName);
            SetDescription(description);
            SetIsFavorite(isFavorite);
            SavedAt = DateTime.UtcNow;
            AccessCount = 0;
            Deleted = false;
        }

        public static SavedCart Create(Guid userId, Guid cartId, string savedCartName, 
            string? description = null, bool isFavorite = false)
            => new(userId, cartId, savedCartName, description, isFavorite);

        public void SetSavedCartName(string savedCartName)
        {
            if (string.IsNullOrWhiteSpace(savedCartName))
                throw new ArgumentException("نام سبد ذخیره شده نباید خالی باشد");
            SavedCartName = savedCartName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string? description)
        {
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetIsFavorite(bool isFavorite)
        {
            IsFavorite = isFavorite;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFavorite()
        {
            IsFavorite = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveFromFavorites()
        {
            IsFavorite = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordAccess()
        {
            LastAccessedAt = DateTime.UtcNow;
            AccessCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string savedCartName, string? description, bool isFavorite, string? updatedBy)
        {
            SetSavedCartName(savedCartName);
            SetDescription(description);
            SetIsFavorite(isFavorite);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این سبد ذخیره شده قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
