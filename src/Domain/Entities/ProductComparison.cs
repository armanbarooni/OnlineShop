using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class ProductComparison : BaseEntity
    {
        public Guid UserId { get; private set; }
        public List<Guid> ProductIds { get; private set; } = new();
        private const int MaxProductsLimit = 5;

        protected ProductComparison() { }

        private ProductComparison(Guid userId)
        {
            UserId = userId;
            ProductIds = new List<Guid>();
            CreatedAt = DateTime.UtcNow;
        }

        public static ProductComparison Create(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty");
            
            return new ProductComparison(userId);
        }

        public void AddProduct(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID cannot be empty");

            if (ProductIds.Contains(productId))
                throw new InvalidOperationException("Product already exists in comparison");

            if (ProductIds.Count >= MaxProductsLimit)
                throw new InvalidOperationException($"Cannot add more than {MaxProductsLimit} products to comparison");

            ProductIds.Add(productId);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveProduct(Guid productId)
        {
            if (!ProductIds.Contains(productId))
                throw new InvalidOperationException("Product not found in comparison");

            ProductIds.Remove(productId);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Clear()
        {
            ProductIds.Clear();
            UpdatedAt = DateTime.UtcNow;
        }

        public bool HasProduct(Guid productId) => ProductIds.Contains(productId);
        public int GetProductCount() => ProductIds.Count;
        public bool IsFull() => ProductIds.Count >= MaxProductsLimit;
    }
}

