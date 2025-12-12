namespace OnlineShop.Infrastructure.Mahak.Models
{
    /// <summary>
    /// Model for sending orders to Mahak
    /// </summary>
    public class MahakOrderModel
    {
        /// <summary>
        /// Order ID from our website (REQUIRED when sending from client)
        /// </summary>
        public long OrderClientId { get; set; }

        /// <summary>
        /// Our Visitor ID in Mahak (REQUIRED) - Get from login response
        /// </summary>
        public int VisitorId { get; set; }

        /// <summary>
        /// Customer's Mahak Person ID (if exists)
        /// </summary>
        public int? PersonId { get; set; }

        /// <summary>
        /// Order type: 201 = Sales Invoice, 299 = Proforma, 202 = Return
        /// </summary>
        public int OrderType { get; set; } = 201; // Sales Invoice

        /// <summary>
        /// Order date
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Delivery date (optional)
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Discount type: 0 = Amount, 1 = Percentage
        /// </summary>
        public int DiscountType { get; set; }

        /// <summary>
        /// Shipping cost
        /// </summary>
        public decimal SendCost { get; set; }

        /// <summary>
        /// Other costs
        /// </summary>
        public decimal OtherCost { get; set; }

        /// <summary>
        /// Settlement type: 1 = Cash, 0 = Credit
        /// </summary>
        public int SettlementType { get; set; } = 1; // Cash

        /// <summary>
        /// Needs immediate delivery
        /// </summary>
        public bool Immediate { get; set; }

        /// <summary>
        /// Order description/notes
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Shipping address (JSON format)
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Latitude for delivery location
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Longitude for delivery location
        /// </summary>
        public double? Longitude { get; set; }
    }

    /// <summary>
    /// Model for order line items
    /// </summary>
    public class MahakOrderDetailModel
    {
        /// <summary>
        /// Order detail ID from our website
        /// </summary>
        public long OrderDetailClientId { get; set; }

        /// <summary>
        /// Item type: 0 = Product, 1 = Income, 2 = Expense
        /// </summary>
        public int ItemType { get; set; } = 0; // Product

        /// <summary>
        /// Our order's client ID (links to MahakOrderModel.OrderClientId)
        /// </summary>
        public long OrderClientId { get; set; }

        /// <summary>
        /// Product ID in Mahak (from MahakMapping)
        /// </summary>
        public int ProductDetailId { get; set; }

        /// <summary>
        /// Store ID in Mahak (REQUIRED)
        /// </summary>
        public int? StoreId { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Quantity (main unit)
        /// </summary>
        public decimal Count1 { get; set; }

        /// <summary>
        /// Quantity (secondary unit) - optional
        /// </summary>
        public decimal Count2 { get; set; }

        /// <summary>
        /// Discount amount for this item
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Discount type: 0 = Amount, 1 = Percentage
        /// </summary>
        public int DiscountType { get; set; }

        /// <summary>
        /// Tax percentage
        /// </summary>
        public decimal TaxPercent { get; set; }

        /// <summary>
        /// Charge percentage
        /// </summary>
        public decimal ChargePercent { get; set; }

        /// <summary>
        /// Item description/notes
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gift quantity
        /// </summary>
        public decimal Gift { get; set; }
    }

    /// <summary>
    /// Request model for SaveAllDataV2 endpoint
    /// </summary>
    public class SaveAllDataRequest
    {
        public List<MahakOrderModel>? Orders { get; set; }
        public List<MahakOrderDetailModel>? OrderDetails { get; set; }
        // Add other entities as needed (people, products, etc.)
    }
}
