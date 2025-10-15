namespace OnlineShop.Application.DTOs.ProductRelation
{
    public class ProductRelationDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid RelatedProductId { get; set; }
        public string RelationType { get; set; } = string.Empty;
        public int Weight { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
