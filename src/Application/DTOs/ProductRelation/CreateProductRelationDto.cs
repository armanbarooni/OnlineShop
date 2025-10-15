namespace OnlineShop.Application.DTOs.ProductRelation
{
    public class CreateProductRelationDto
    {
        public Guid ProductId { get; set; }
        public Guid RelatedProductId { get; set; }
        public string RelationType { get; set; } = string.Empty;
        public int Weight { get; set; } = 1;
    }
}
