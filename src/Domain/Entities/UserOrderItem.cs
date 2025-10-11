using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class UserOrderItem : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public string? ProductDescription { get; private set; }
        public string? ProductSku { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice { get; private set; }
        public decimal? DiscountAmount { get; private set; }
        public string? Notes { get; private set; }

        // Navigation Properties
        public virtual UserOrder Order { get; private set; } = null!;
        public virtual Product Product { get; private set; } = null!;

        protected UserOrderItem() { }

        private UserOrderItem(Guid orderId, Guid productId, string productName, int quantity, 
            decimal unitPrice, decimal totalPrice)
        {
            OrderId = orderId;
            ProductId = productId;
            SetProductName(productName);
            SetQuantity(quantity);
            SetUnitPrice(unitPrice);
            SetTotalPrice(totalPrice);
            Deleted = false;
        }

        public static UserOrderItem Create(Guid orderId, Guid productId, string productName, 
            int quantity, decimal unitPrice, decimal totalPrice)
            => new(orderId, productId, productName, quantity, unitPrice, totalPrice);

        public void SetProductName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("نام محصول نباید خالی باشد");
            ProductName = productName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetProductDescription(string? productDescription)
        {
            ProductDescription = productDescription?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetProductSku(string? productSku)
        {
            ProductSku = productSku?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("تعداد باید بزرگتر از صفر باشد");
            Quantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
                throw new ArgumentException("قیمت واحد نمی‌تواند منفی باشد");
            UnitPrice = unitPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetTotalPrice(decimal totalPrice)
        {
            if (totalPrice < 0)
                throw new ArgumentException("قیمت کل نمی‌تواند منفی باشد");
            TotalPrice = totalPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDiscountAmount(decimal? discountAmount)
        {
            if (discountAmount.HasValue && discountAmount.Value < 0)
                throw new ArgumentException("مبلغ تخفیف نمی‌تواند منفی باشد");
            DiscountAmount = discountAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNotes(string? notes)
        {
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string productName, string? productDescription, string? productSku, 
            int quantity, decimal unitPrice, decimal totalPrice, decimal? discountAmount, 
            string? notes, string? updatedBy)
        {
            SetProductName(productName);
            SetProductDescription(productDescription);
            SetProductSku(productSku);
            SetQuantity(quantity);
            SetUnitPrice(unitPrice);
            SetTotalPrice(totalPrice);
            SetDiscountAmount(discountAmount);
            SetNotes(notes);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این آیتم سفارش قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
