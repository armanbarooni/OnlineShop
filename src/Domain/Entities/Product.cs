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
        public Guid? BrandId { get; private set; }
        public string? Gender { get; private set; } // Male, Female, Kids, Unisex
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
        public bool DeletedByMahak { get; private set; }

        // Navigation Properties
        public virtual ProductCategory? Category { get; private set; }
        public virtual Unit? Unit { get; private set; }
        public virtual Brand? Brand { get; private set; }
        public virtual ICollection<ProductDetail> ProductDetails { get; private set; } = new List<ProductDetail>();
        public virtual ICollection<ProductImage> ProductImages { get; private set; } = new List<ProductImage>();
        public virtual ICollection<ProductReview> ProductReviews { get; private set; } = new List<ProductReview>();
        public virtual ICollection<ProductInventory> ProductInventories { get; private set; } = new List<ProductInventory>();
        public virtual ICollection<ProductVariant> ProductVariants { get; private set; } = new List<ProductVariant>();
        public virtual ICollection<ProductMaterial> ProductMaterials { get; private set; } = new List<ProductMaterial>();
        public virtual ICollection<ProductSeason> ProductSeasons { get; private set; } = new List<ProductSeason>();
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
            DeletedByMahak = false;
        }

        public static Product Create(string name, string description, decimal price, int stockQuantity, long? mahakClientId=null, int? mahakId=null)
            => new(name, description, price, stockQuantity, mahakClientId, mahakId);

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ù†Ø§Ù… Ù…Ø­ØµÙˆÙ„ Ù†Ø¨Ø§ÛŒØ¯ Ø®Ø§Ù„ÛŒ Ø¨Ø§Ø´Ø¯");
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
            if (price < 0)
                throw new ArgumentException("Ù‚ÛŒÙ…Øª Ù…Ø­ØµÙˆÙ„ Ù†Ù…ÛŒâ€ŒØªÙˆØ§Ù†Ø¯ Ù…Ù†ÙÛŒ Ø¨Ø§Ø´Ø¯");
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStockQuantity(int qty)
            => SetStockQuantity(qty, DateTime.UtcNow);

        public void SetStockQuantity(int qty, DateTime updatedAt)
        {
            if (qty < 0)
                throw new ArgumentException("ØªØ¹Ø¯Ø§Ø¯ Ù…ÙˆØ¬ÙˆØ¯ÛŒ Ù†Ù…ÛŒâ€ŒØªÙˆØ§Ù†Ø¯ Ù…Ù†ÙÛŒ Ø¨Ø§Ø´Ø¯");
            StockQuantity = qty;
            UpdatedAt = updatedAt;
        }

        public void ReduceStock(int quantity, DateTime updatedAt)
        {
            if (quantity <= 0)
                throw new ArgumentException("مقدار کاهش موجودی باید مثبت باشد");
            if (quantity > StockQuantity)
                throw new InvalidOperationException("موجودی کافی نیست");

            StockQuantity -= quantity;
            UpdatedAt = updatedAt;
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

        public void SetBrandId(Guid? brandId)
        {
            BrandId = brandId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetGender(string? gender)
        {
            if (gender != null)
            {
                var validGenders = new[] { "Male", "Female", "Kids", "Unisex" };
                if (!validGenders.Contains(gender, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException("Ø¬Ù†Ø³ÛŒØª Ø¨Ø§ÛŒØ¯ ÛŒÚ©ÛŒ Ø§Ø² Ù…Ù‚Ø§Ø¯ÛŒØ± Male, Female, Kids, Unisex Ø¨Ø§Ø´Ø¯");
            }
            Gender = gender?.Trim();
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
                throw new ArgumentException("ÙˆØ²Ù† Ù†Ù…ÛŒâ€ŒØªÙˆØ§Ù†Ø¯ Ù…Ù†ÙÛŒ Ø¨Ø§Ø´Ø¯");
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

        public void Activate()
        {
            SetIsActive(true);
        }

        public void Deactivate()
        {
            SetIsActive(false);
        }

        public void SetIsFeatured(bool isFeatured)
        {
            IsFeatured = isFeatured;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsFeatured()
        {
            SetIsFeatured(true);
        }

        public void RemoveFromFeatured()
        {
            SetIsFeatured(false);
        }

        public void IncrementViewCount()
        {
            ViewCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSalePrice(decimal? salePrice)
        {
            if (salePrice.HasValue && salePrice.Value < 0)
                throw new ArgumentException("Ù‚ÛŒÙ…Øª ÙØ±ÙˆØ´ Ù†Ù…ÛŒâ€ŒØªÙˆØ§Ù†Ø¯ Ù…Ù†ÙÛŒ Ø¨Ø§Ø´Ø¯");
            SalePrice = salePrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSalePeriod(DateTime? saleStartDate, DateTime? saleEndDate)
        {
            if (saleStartDate.HasValue && saleEndDate.HasValue && saleStartDate.Value >= saleEndDate.Value)
                throw new ArgumentException("ØªØ§Ø±ÛŒØ® Ø´Ø±ÙˆØ¹ ÙØ±ÙˆØ´ Ø¨Ø§ÛŒØ¯ Ù‚Ø¨Ù„ Ø§Ø² ØªØ§Ø±ÛŒØ® Ù¾Ø§ÛŒØ§Ù† Ø¨Ø§Ø´Ø¯");
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
                throw new InvalidOperationException("Ø§ÛŒÙ† Ù…Ø­ØµÙˆÙ„ Ù‚Ø¨Ù„Ø§Ù‹ Ø­Ø°Ù Ø´Ø¯Ù‡ Ø§Ø³Øª.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
        public void SetDeletedByMahak(bool deletedByMahak)
        {
            DeletedByMahak = deletedByMahak;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

