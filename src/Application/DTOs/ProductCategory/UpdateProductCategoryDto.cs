using System.Text.Json.Serialization;

namespace OnlineShop.Application.DTOs.ProductCategory
{
    public class UpdateProductCategoryDto
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
