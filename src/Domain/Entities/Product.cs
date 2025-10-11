using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public Guid? CategoryId { get; private set; }
        public Guid? UnitId { get; private set; }
        public string? Sku { get; private set; }
        public string? Barcode { get; private set; }
        public decimal? Weight { get; private set; }
        public string? Dimensions { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool IsFeatured { get; private set; }
        public int ViewCount { get; private set; }
        public decimal? SalePrice { get; private set; }
        public DateTime? SaleStartDate { get; private set; }
        public DateTime? SaleEndDate { get; private set; }

        // Navigation Properties
        public virtual ProductCategory? Category { get; private set; }
        public virtual Unit? Unit { get; private set; }
        public virtual ICollection<ProductDetail> ProductDetails { get; private set; } = new List<ProductDetail>();
        public virtual ICollection<ProductImage> ProductImages { get; private set; } = new List<ProductImage>();
        public virtual ICollection<ProductReview> ProductReviews { get; private set; } = new List<ProductReview>();
        public virtual ICollection<ProductInventory> ProductInventories { get; private set; } = new List<ProductInventory>();
        public virtual ICollection<Wishlist> Wishlists { get; private set; } = new List<Wishlist>();
        public virtual ICollection<UserOrderItem> OrderItems { get; private set; } = new List<UserOrderItem>();
        public virtual ICollection<CartItem> CartItems { get; private set; } = new List<CartItem>();

        protected Product() { }

        private Product(string name, string description, decimal price, int stockQuantity, long? mahakClientId, int? mahakId = null)
        {
            SetName(name);
            SetDescription(description);
            SetPrice(price);
            SetStockQuantity(stockQuantity);
            MahakClientId = mahakClientId;
            MahakId = mahakId;
            Deleted = false;
        }

        public static Product Create(string name, string description, decimal price, int stockQuantity, long? mahakClientId=null, int? mahakId=null)
            => new(name, description, price, stockQuantity, mahakClientId, mahakId);

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام محصول نباید خالی باشد");
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string description)
        {
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPrice(decimal price)
        {
            if (price <= 0)
                throw new ArgumentException("قیمت محصول باید بزرگتر از صفر باشد");
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStockQuantity(int qty)
        {
            if (qty < 0)
                throw new ArgumentException("تعداد موجودی نمی‌تواند منفی باشد");
            StockQuantity = qty;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string description, decimal price, int qty, string? updatedBy)
        {
            SetName(name);
            SetDescription(description);
            SetPrice(price);
            SetStockQuantity(qty);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCategoryId(Guid? categoryId)
        {
            CategoryId = categoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetUnitId(Guid? unitId)
        {
            UnitId = unitId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSku(string? sku)
        {
            Sku = sku?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetBarcode(string? barcode)
        {
            Barcode = barcode?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetWeight(decimal? weight)
        {
            if (weight.HasValue && weight.Value < 0)
                throw new ArgumentException("وزن نمی‌تواند منفی باشد");
            Weight = weight;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDimensions(string? dimensions)
        {
            Dimensions = dimensions?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetIsActive(bool isActive)
        {
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetIsFeatured(bool isFeatured)
        {
            IsFeatured = isFeatured;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncrementViewCount()
        {
            ViewCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSalePrice(decimal? salePrice)
        {
            if (salePrice.HasValue && salePrice.Value < 0)
                throw new ArgumentException("قیمت فروش نمی‌تواند منفی باشد");
            SalePrice = salePrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSalePeriod(DateTime? saleStartDate, DateTime? saleEndDate)
        {
            if (saleStartDate.HasValue && saleEndDate.HasValue && saleStartDate.Value >= saleEndDate.Value)
                throw new ArgumentException("تاریخ شروع فروش باید قبل از تاریخ پایان باشد");
            SaleStartDate = saleStartDate;
            SaleEndDate = saleEndDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsOnSale()
        {
            if (!SalePrice.HasValue) return false;
            var now = DateTime.UtcNow;
            return (!SaleStartDate.HasValue || SaleStartDate.Value <= now) &&
                   (!SaleEndDate.HasValue || SaleEndDate.Value >= now);
        }

        public decimal GetCurrentPrice()
        {
            return IsOnSale() && SalePrice.HasValue ? SalePrice.Value : Price;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این محصول قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}


