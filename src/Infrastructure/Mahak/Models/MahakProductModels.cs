namespace OnlineShop.Infrastructure.Mahak.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public int ProductClientId { get; set; }
        public int ProductCode { get; set; }
        public int ProductCategoryId { get; set; }
        public string? Name { get; set; }
        public string? UnitName { get; set; }
        public string? UnitName2 { get; set; }
        public decimal UnitRatio { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal ChargePercent { get; set; }
        public string? Description { get; set; }
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
        
        // Add other fields as they appear in the log/docs if needed
        public decimal Price { get; set; } // Note: Price might be in a separate list or here depending on schema. 
        // Checking bugs/mahak.txt logs... In the log, Product properties shown are:
        // ProductId, ProductClientId, ProductCode, ProductCategoryId, Name, UnitName, ..., Deleted, RowVersion.
        // I don't see "Price" in the log's Product object. It might be in "ProductProperties" or "ProductDetails".
        // Let's stick to the log's fields + essential ones.
    }
}
