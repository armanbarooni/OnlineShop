namespace OnlineShop.Domain.Enums
{
    /// <summary>
    /// Order status enumeration
    /// Represents the current state of an order in the fulfillment process
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order has been placed and is awaiting processing
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Order is being processed and prepared
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Order has been packed and is ready for shipment
        /// </summary>
        Packed = 2,

        /// <summary>
        /// Order has been shipped and is in transit
        /// </summary>
        Shipped = 3,

        /// <summary>
        /// Order is out for delivery to the customer
        /// </summary>
        OutForDelivery = 4,

        /// <summary>
        /// Order has been successfully delivered to the customer
        /// </summary>
        Delivered = 5,

        /// <summary>
        /// Order has been cancelled (before or after processing)
        /// </summary>
        Cancelled = 6,

        /// <summary>
        /// Order has been returned by the customer
        /// </summary>
        Returned = 7
    }
}
