using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }

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

        public void Update(string name, string description, decimal price, int qty, int? updatedBy)
        {
            SetName(name);
            SetDescription(description);
            SetPrice(price);
            SetStockQuantity(qty);
            UpdatedBy = updatedBy ?? 1;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(int? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این محصول قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy ?? 1;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}


