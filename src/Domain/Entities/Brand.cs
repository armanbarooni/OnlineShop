using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Brand/Manufacturer entity for products
    /// </summary>
    public class Brand : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? LogoUrl { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;
        public string? Website { get; private set; }
        public int DisplayOrder { get; private set; }

        // Navigation Properties
        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        protected Brand() { }

        private Brand(string name, string? logoUrl = null, string? description = null)
        {
            SetName(name);
            SetLogoUrl(logoUrl);
            SetDescription(description);
            IsActive = true;
            DisplayOrder = 0;
            Deleted = false;
        }

        public static Brand Create(string name, string? logoUrl = null, string? description = null)
            => new(name, logoUrl, description);

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام برند نباید خالی باشد");
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetLogoUrl(string? logoUrl)
        {
            LogoUrl = logoUrl?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string? description)
        {
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetWebsite(string? website)
        {
            Website = website?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0)
                throw new ArgumentException("ترتیب نمایش نمی‌تواند منفی باشد");
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? logoUrl, string? description, string? website, int displayOrder, string? updatedBy)
        {
            SetName(name);
            SetLogoUrl(logoUrl);
            SetDescription(description);
            SetWebsite(website);
            SetDisplayOrder(displayOrder);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این برند قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

