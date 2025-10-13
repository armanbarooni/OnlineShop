using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Junction table for Product-Season many-to-many relationship
    /// </summary>
    public class ProductSeason : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Guid SeasonId { get; private set; }

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;
        public virtual Season Season { get; private set; } = null!;

        protected ProductSeason() { }

        private ProductSeason(Guid productId, Guid seasonId)
        {
            ProductId = productId;
            SeasonId = seasonId;
            Deleted = false;
        }

        public static ProductSeason Create(Guid productId, Guid seasonId)
            => new(productId, seasonId);
    }
}

