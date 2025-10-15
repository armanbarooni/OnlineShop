using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class UserOrder : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public string OrderStatus { get; private set; } = "Pending";
        public decimal SubTotal { get; private set; }
        public decimal TaxAmount { get; private set; }
        public decimal ShippingAmount { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string Currency { get; private set; } = "IRR";
        public string? Notes { get; private set; }
        public DateTime? ShippedAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }
        public string? TrackingNumber { get; private set; }
        public DateTime? EstimatedDeliveryDate { get; private set; }
        public DateTime? ActualDeliveryDate { get; private set; }
        public Guid? ShippingAddressId { get; private set; }
        public Guid? BillingAddressId { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual UserAddress? ShippingAddress { get; private set; }
        public virtual UserAddress? BillingAddress { get; private set; }
        public virtual ICollection<UserOrderItem> OrderItems { get; private set; } = new List<UserOrderItem>();
        public virtual ICollection<UserPayment> Payments { get; private set; } = new List<UserPayment>();

        protected UserOrder() { }

        private UserOrder(Guid userId, string orderNumber, decimal subTotal, decimal taxAmount, 
            decimal shippingAmount, decimal discountAmount, decimal totalAmount, string currency)
        {
            UserId = userId;
            SetOrderNumber(orderNumber);
            SetSubTotal(subTotal);
            SetTaxAmount(taxAmount);
            SetShippingAmount(shippingAmount);
            SetDiscountAmount(discountAmount);
            SetTotalAmount(totalAmount);
            SetCurrency(currency);
            OrderStatus = "Pending";
            Deleted = false;
        }

        public static UserOrder Create(Guid userId, string orderNumber, decimal subTotal, decimal taxAmount, 
            decimal shippingAmount, decimal discountAmount, decimal totalAmount, string currency = "IRR")
            => new(userId, orderNumber, subTotal, taxAmount, shippingAmount, discountAmount, totalAmount, currency);

        public void SetOrderNumber(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                throw new ArgumentException("شماره سفارش نباید خالی باشد");
            OrderNumber = orderNumber.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSubTotal(decimal subTotal)
        {
            if (subTotal < 0)
                throw new ArgumentException("مجموع جزئی نمی‌تواند منفی باشد");
            SubTotal = subTotal;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetTaxAmount(decimal taxAmount)
        {
            if (taxAmount < 0)
                throw new ArgumentException("مبلغ مالیات نمی‌تواند منفی باشد");
            TaxAmount = taxAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetShippingAmount(decimal shippingAmount)
        {
            if (shippingAmount < 0)
                throw new ArgumentException("مبلغ ارسال نمی‌تواند منفی باشد");
            ShippingAmount = shippingAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDiscountAmount(decimal discountAmount)
        {
            if (discountAmount < 0)
                throw new ArgumentException("مبلغ تخفیف نمی‌تواند منفی باشد");
            DiscountAmount = discountAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetTotalAmount(decimal totalAmount)
        {
            if (totalAmount < 0)
                throw new ArgumentException("مجموع کل نمی‌تواند منفی باشد");
            TotalAmount = totalAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("واحد پول نباید خالی باشد");
            Currency = currency.Trim().ToUpper();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNotes(string? notes)
        {
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetShippingAddress(Guid? shippingAddressId)
        {
            ShippingAddressId = shippingAddressId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetBillingAddress(Guid? billingAddressId)
        {
            BillingAddressId = billingAddressId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetTrackingNumber(string? trackingNumber)
        {
            TrackingNumber = trackingNumber?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Confirm()
        {
            OrderStatus = "Confirmed";
            UpdatedAt = DateTime.UtcNow;
        }

        public void StartProcessing(string? updatedBy = null)
        {
            if (OrderStatus != "Pending")
                throw new InvalidOperationException($"فقط سفارشات در حالت 'Pending' می‌توانند به حالت 'Processing' تغییر کنند. وضعیت فعلی: {OrderStatus}");
            
            OrderStatus = "Processing";
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Ship(string? trackingNumber)
        {
            if (OrderStatus != "Processing" && OrderStatus != "Pending")
                throw new InvalidOperationException($"فقط سفارشات در حالت 'Processing' یا 'Pending' می‌توانند ارسال شوند. وضعیت فعلی: {OrderStatus}");

            OrderStatus = "Shipped";
            SetTrackingNumber(trackingNumber);
            ShippedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsShipped(string trackingNumber, string? updatedBy = null)
        {
            Ship(trackingNumber);
            UpdatedBy = updatedBy;
        }

        public void Deliver()
        {
            if (OrderStatus != "Shipped")
                throw new InvalidOperationException($"فقط سفارشات ارسال شده می‌توانند تحویل داده شوند. وضعیت فعلی: {OrderStatus}");

            OrderStatus = "Delivered";
            DeliveredAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsDelivered(string? updatedBy = null)
        {
            Deliver();
            UpdatedBy = updatedBy;
        }

        public void SetEstimatedDeliveryDate(DateTime estimatedDeliveryDate)
        {
            if (estimatedDeliveryDate <= DateTime.UtcNow)
                throw new ArgumentException("تاریخ تحویل پیش‌بینی شده باید در آینده باشد");
            EstimatedDeliveryDate = estimatedDeliveryDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetActualDeliveryDate(DateTime actualDeliveryDate)
        {
            if (actualDeliveryDate > DateTime.UtcNow)
                throw new ArgumentException("تاریخ تحویل واقعی نمی‌تواند در آینده باشد");
            ActualDeliveryDate = actualDeliveryDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(string newStatus, string? note = null, string? updatedBy = null)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("وضعیت سفارش نمی‌تواند خالی باشد");

            OrderStatus = newStatus.Trim();
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(string? cancellationReason, string? updatedBy = null)
        {
            if (OrderStatus == "Delivered")
                throw new InvalidOperationException("سفارشات تحویل داده شده نمی‌توانند لغو شوند");

            OrderStatus = "Cancelled";
            CancellationReason = cancellationReason?.Trim();
            CancelledAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(decimal subTotal, decimal taxAmount, decimal shippingAmount, 
            decimal discountAmount, decimal totalAmount, string? notes, string? updatedBy)
        {
            SetSubTotal(subTotal);
            SetTaxAmount(taxAmount);
            SetShippingAmount(shippingAmount);
            SetDiscountAmount(discountAmount);
            SetTotalAmount(totalAmount);
            SetNotes(notes);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این سفارش قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
