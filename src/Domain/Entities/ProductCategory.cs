using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public Guid? ParentCategoryId { get; private set; }
        public int Level { get; private set; }

        // Navigation Properties
        public virtual ProductCategory? ParentCategory { get; private set; }
        public virtual ICollection<ProductCategory> SubCategories { get; private set; } = new List<ProductCategory>();

        protected ProductCategory() { }

        private ProductCategory(string name, string description, long mahakClientId, int mahakId, Guid? parentCategoryId = null)
        {
            SetName(name);
            SetDescription(description);
            MahakClientId = mahakClientId;
            MahakId = mahakId;
            ParentCategoryId = parentCategoryId;
            Level = 0; // Will be calculated based on parent
            Deleted = false;
        }

        public static ProductCategory Create(string name, string description, long mahakClientId, int mahakId, Guid? parentCategoryId = null)
            => new(name, description, mahakClientId, mahakId, parentCategoryId);

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

        public void SetParentCategory(Guid? parentCategoryId)
        {
            ParentCategoryId = parentCategoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetLevel(int level)
        {
            if (level < 0)
                throw new ArgumentException("Level cannot be negative");
            Level = level;
        }

        public string GetFullPath()
        {
            if (ParentCategory == null)
                return Name;
            
            return $"{ParentCategory.GetFullPath()} > {Name}";
        }
    }
}


