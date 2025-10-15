namespace OnlineShop.Application.DTOs.ProductRelation
{
    public class UpdateProductRelationDto
    {
        public Guid Id { get; set; }
        public string RelationType { get; set; } = string.Empty;
        public int Weight { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }
}
