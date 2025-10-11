using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice { get; private set; }
        public string? Notes { get; private set; }

        // Navigation Properties
        public virtual Cart Cart { get; private set; } = null!;
        public virtual Product Product { get; private set; } = null!;

        protected CartItem() { }

        private CartItem(Guid cartId, Guid productId, int quantity, decimal unitPrice, decimal totalPrice)
        {
            CartId = cartId;
            ProductId = productId;
            SetQuantity(quantity);
            SetUnitPrice(unitPrice);
            SetTotalPrice(totalPrice);
            Deleted = false;
        }

        public static CartItem Create(Guid cartId, Guid productId, int quantity, decimal unitPrice, decimal totalPrice)
            => new(cartId, productId, quantity, unitPrice, totalPrice);

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

        public void SetNotes(string? notes)
        {
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateQuantity(int newQuantity)
        {
            SetQuantity(newQuantity);
            // Recalculate total price
            SetTotalPrice(UnitPrice * newQuantity);
        }

        public void UpdatePrice(decimal newUnitPrice)
        {
            SetUnitPrice(newUnitPrice);
            // Recalculate total price
            SetTotalPrice(newUnitPrice * Quantity);
        }

        public void Update(int quantity, decimal unitPrice, string? notes, string? updatedBy)
        {
            SetQuantity(quantity);
            SetUnitPrice(unitPrice);
            SetTotalPrice(unitPrice * quantity);
            SetNotes(notes);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این آیتم سبد خرید قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
