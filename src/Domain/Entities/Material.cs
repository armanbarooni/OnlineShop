using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Material/Fabric type for products (Cotton, Polyester, Silk, etc.)
    /// </summary>
    public class Material : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation Properties (many-to-many with Product)
        public virtual ICollection<ProductMaterial> ProductMaterials { get; private set; } = new List<ProductMaterial>();

        protected Material() { }

        private Material(string name, string? description = null)
        {
            SetName(name);
            SetDescription(description);
            IsActive = true;
            Deleted = false;
        }

        public static Material Create(string name, string? description = null)
            => new(name, description);

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام متریال نباید خالی باشد");
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string? description)
        {
            Description = description?.Trim();
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

        public void Update(string name, string? description, string? updatedBy)
        {
            SetName(name);
            SetDescription(description);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این متریال قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

