using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class ProductInventory : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public int AvailableQuantity { get; private set; }
        public int ReservedQuantity { get; private set; }
        public int SoldQuantity { get; private set; }
        public decimal? CostPrice { get; private set; }
        public decimal? SellingPrice { get; private set; }
        public DateTime LastSyncAt { get; private set; }
        public string? SyncStatus { get; private set; }
        public string? SyncError { get; private set; }

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;

        protected ProductInventory() { }

        private ProductInventory(Guid productId, int availableQuantity, int reservedQuantity, 
            int soldQuantity, decimal? costPrice, decimal? sellingPrice)
        {
            ProductId = productId;
            SetAvailableQuantity(availableQuantity);
            SetReservedQuantity(reservedQuantity);
            SetSoldQuantity(soldQuantity);
            SetCostPrice(costPrice);
            SetSellingPrice(sellingPrice);
            LastSyncAt = DateTime.UtcNow;
            SyncStatus = "Synced";
            Deleted = false;
        }

        public static ProductInventory Create(Guid productId, int availableQuantity, int reservedQuantity = 0, 
            int soldQuantity = 0, decimal? costPrice = null, decimal? sellingPrice = null)
            => new(productId, availableQuantity, reservedQuantity, soldQuantity, costPrice, sellingPrice);

        public void SetAvailableQuantity(int availableQuantity)
        {
            if (availableQuantity < 0)
                throw new ArgumentException("تعداد موجودی نمی‌تواند منفی باشد");
            AvailableQuantity = availableQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetReservedQuantity(int reservedQuantity)
        {
            if (reservedQuantity < 0)
                throw new ArgumentException("تعداد رزرو شده نمی‌تواند منفی باشد");
            ReservedQuantity = reservedQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSoldQuantity(int soldQuantity)
        {
            if (soldQuantity < 0)
                throw new ArgumentException("تعداد فروخته شده نمی‌تواند منفی باشد");
            SoldQuantity = soldQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCostPrice(decimal? costPrice)
        {
            if (costPrice.HasValue && costPrice.Value < 0)
                throw new ArgumentException("قیمت تمام شده نمی‌تواند منفی باشد");
            CostPrice = costPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSellingPrice(decimal? sellingPrice)
        {
            if (sellingPrice.HasValue && sellingPrice.Value < 0)
                throw new ArgumentException("قیمت فروش نمی‌تواند منفی باشد");
            SellingPrice = sellingPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateSyncStatus(string status, string? error = null)
        {
            SyncStatus = status;
            SyncError = error;
            LastSyncAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateInventory(int availableQuantity, int reservedQuantity, int soldQuantity, 
            decimal? costPrice, decimal? sellingPrice, string? updatedBy)
        {
            SetAvailableQuantity(availableQuantity);
            SetReservedQuantity(reservedQuantity);
            SetSoldQuantity(soldQuantity);
            SetCostPrice(costPrice);
            SetSellingPrice(sellingPrice);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public int GetTotalQuantity() => AvailableQuantity + ReservedQuantity + SoldQuantity;

        public int GetAvailableStock() => AvailableQuantity - ReservedQuantity;

        public void ReserveQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("مقدار رزرو باید بزرگتر از صفر باشد");

            var availableStock = GetAvailableStock();
            if (availableStock < quantity)
                throw new InvalidOperationException($"موجودی کافی نیست. موجودی قابل دسترس: {availableStock}، درخواستی: {quantity}");

            ReservedQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ReleaseReservedQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("مقدار آزادسازی باید بزرگتر از صفر باشد");

            if (ReservedQuantity < quantity)
                throw new InvalidOperationException($"مقدار رزرو شده کافی نیست. رزرو شده: {ReservedQuantity}, درخواستی: {quantity}");

            ReservedQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void CommitSale(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("مقدار فروش باید بزرگتر از صفر باشد");

            if (ReservedQuantity < quantity)
                throw new InvalidOperationException($"مقدار رزرو شده کافی نیست. رزرو شده: {ReservedQuantity}, درخواستی: {quantity}");

            ReservedQuantity -= quantity;
            SoldQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("مقدار اضافه شدن باید بزرگتر از صفر باشد");

            AvailableQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این موجودی محصول قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
