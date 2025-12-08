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
    }

    public class ProductDetailModel
    {
        public int ProductDetailId { get; set; }
        public int ProductDetailClientId { get; set; }
        public int ProductDetailCode { get; set; }
        public int ProductId { get; set; }
        public int ProductClientId { get; set; }
        public int ProductCode { get; set; }
        public string? Properties { get; set; }
        public string? Barcode { get; set; }
        public decimal Price1 { get; set; }
        public decimal Price2 { get; set; }
        public decimal Price3 { get; set; }
        public decimal Price4 { get; set; }
        public decimal Price5 { get; set; }
        public decimal Discount { get; set; }
        public int DefaultSellPriceLevel { get; set; }
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
    }

    public class ProductDetailStoreAssetModel
    {
        public int ProductDetailStoreAssetId { get; set; }
        public int ProductDetailId { get; set; }
        public int StoreId { get; set; }
        public decimal Count1 { get; set; }
        public decimal Count2 { get; set; }
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
    }

    public class ProductCategoryModel
    {
        public int ProductCategoryId { get; set; }
        public int ProductCategoryClientId { get; set; }
        public int ProductCategoryCode { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool Deleted { get; set; }
        public long RowVersion { get; set; }
    }
}
