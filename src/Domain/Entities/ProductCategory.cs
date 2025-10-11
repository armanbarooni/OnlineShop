using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        protected ProductCategory() { }

        private ProductCategory(string name, string description, long mahakClientId, int mahakId)
        {
            SetName(name);
            SetDescription(description);
            MahakClientId = mahakClientId;
            MahakId = mahakId;
            Deleted = false;
        }

        public static ProductCategory Create(string name, string description, long mahakClientId, int mahakId)
            => new(name, description, mahakClientId, mahakId);

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty");
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string description)
        {
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string description, string? updatedBy)
        {
            SetName(name);
            SetDescription(description);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("This category is already deleted.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}


