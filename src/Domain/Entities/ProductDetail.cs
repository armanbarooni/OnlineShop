using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class ProductDetail : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string Key { get; private set; } = string.Empty;
        public string Value { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public int DisplayOrder { get; private set; }

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;

        protected ProductDetail() { }

        private ProductDetail(Guid productId, string key, string value, string? description, int displayOrder)
        {
            ProductId = productId;
            SetKey(key);
            SetValue(value);
            SetDescription(description);
            SetDisplayOrder(displayOrder);
            Deleted = false;
        }

        public static ProductDetail Create(Guid productId, string key, string value, string? description = null, int displayOrder = 0)
            => new(productId, key, value, description, displayOrder);

        public void SetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("کلید مشخصات محصول نباید خالی باشد");
            Key = key.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("مقدار مشخصات محصول نباید خالی باشد");
            Value = value.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string? description)
        {
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0)
                throw new ArgumentException("ترتیب نمایش نمی‌تواند منفی باشد");
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string key, string value, string? description, int displayOrder, string? updatedBy)
        {
            SetKey(key);
            SetValue(value);
            SetDescription(description);
            SetDisplayOrder(displayOrder);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این مشخصات محصول قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
