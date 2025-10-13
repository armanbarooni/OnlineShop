using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Junction table for Product-Material many-to-many relationship
    /// </summary>
    public class ProductMaterial : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Guid MaterialId { get; private set; }

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;
        public virtual Material Material { get; private set; } = null!;

        protected ProductMaterial() { }

        private ProductMaterial(Guid productId, Guid materialId)
        {
            ProductId = productId;
            MaterialId = materialId;
            Deleted = false;
        }

        public static ProductMaterial Create(Guid productId, Guid materialId)
            => new(productId, materialId);
    }
}

