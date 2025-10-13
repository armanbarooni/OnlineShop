using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Product variant for size/color combinations
    /// Essential for fashion ecommerce
    /// </summary>
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string Size { get; private set; } = string.Empty; // XS, S, M, L, XL, XXL, etc.
        public string Color { get; private set; } = string.Empty; // Red, Blue, Black, etc.
        public string SKU { get; private set; } = string.Empty; // Unique identifier for this variant
        public string? Barcode { get; private set; }
        public int StockQuantity { get; private set; }
        public decimal? AdditionalPrice { get; private set; } // Extra cost for this variant (e.g., +$5 for XL)
        public bool IsAvailable { get; private set; } = true;
        public int DisplayOrder { get; private set; }

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;

        protected ProductVariant() { }

        private ProductVariant(Guid productId, string size, string color, string sku, int stockQuantity)
        {
            ProductId = productId;
            SetSize(size);
            SetColor(color);
            SetSKU(sku);
            SetStockQuantity(stockQuantity);
            IsAvailable = true;
            DisplayOrder = 0;
            Deleted = false;
        }

        public static ProductVariant Create(Guid productId, string size, string color, string sku, int stockQuantity)
            => new(productId, size, color, sku, stockQuantity);

        public void SetSize(string size)
        {
            if (string.IsNullOrWhiteSpace(size))
                throw new ArgumentException("سایز نباید خالی باشد");
            Size = size.Trim().ToUpper(); // Normalize to uppercase
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                throw new ArgumentException("رنگ نباید خالی باشد");
            Color = color.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSKU(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU نباید خالی باشد");
            SKU = sku.Trim().ToUpper();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetBarcode(string? barcode)
        {
            Barcode = barcode?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStockQuantity(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("موجودی نمی‌تواند منفی باشد");
            StockQuantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("مقدار افزایش موجودی باید مثبت باشد");
            StockQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ReduceStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("مقدار کاهش موجودی باید مثبت باشد");
            if (quantity > StockQuantity)
                throw new InvalidOperationException("موجودی کافی نیست");
            StockQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAdditionalPrice(decimal? additionalPrice)
        {
            if (additionalPrice.HasValue && additionalPrice.Value < 0)
                throw new ArgumentException("قیمت اضافی نمی‌تواند منفی باشد");
            AdditionalPrice = additionalPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0)
                throw new ArgumentException("ترتیب نمایش نمی‌تواند منفی باشد");
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string size, string color, string sku, int stockQuantity, decimal? additionalPrice, string? updatedBy)
        {
            SetSize(size);
            SetColor(color);
            SetSKU(sku);
            SetStockQuantity(stockQuantity);
            SetAdditionalPrice(additionalPrice);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این تنوع محصول قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsInStock()
        {
            return IsAvailable && StockQuantity > 0;
        }

        public decimal GetFinalPrice(decimal basePrice)
        {
            return basePrice + (AdditionalPrice ?? 0);
        }
    }
}

