using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Product relation entity for managing related products
    /// Supports Similar and Complement relationship types
    /// </summary>
    public class ProductRelation : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Guid RelatedProductId { get; private set; }
        public string RelationType { get; private set; } = string.Empty; // "Similar", "Complement"
        public int Weight { get; private set; } = 1; // 1-10, higher means stronger relation
        public bool IsActive { get; private set; } = true;

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;
        public virtual Product RelatedProduct { get; private set; } = null!;

        protected ProductRelation() { }

        private ProductRelation(Guid productId, Guid relatedProductId, string relationType, int weight = 1)
        {
            ProductId = productId;
            RelatedProductId = relatedProductId;
            SetRelationType(relationType);
            SetWeight(weight);
            IsActive = true;
            Deleted = false;
        }

        public static ProductRelation Create(Guid productId, Guid relatedProductId, string relationType, int weight = 1)
            => new(productId, relatedProductId, relationType, weight);

        public void SetRelationType(string relationType)
        {
            if (string.IsNullOrWhiteSpace(relationType))
                throw new ArgumentException("نوع رابطه نباید خالی باشد");

            var validTypes = new[] { "Similar", "Complement" };
            if (!validTypes.Contains(relationType, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("نوع رابطه باید Similar یا Complement باشد");

            RelationType = relationType;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetWeight(int weight)
        {
            if (weight < 1 || weight > 10)
                throw new ArgumentException("وزن رابطه باید بین 1 تا 10 باشد");
            Weight = weight;
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

        public void Update(string relationType, int weight, string? updatedBy)
        {
            SetRelationType(relationType);
            SetWeight(weight);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این رابطه قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
