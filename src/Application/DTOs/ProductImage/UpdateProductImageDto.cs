using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.ProductImage
{
    public class UpdateProductImageDto
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public string? Title { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
        public long FileSize { get; set; }
        public string? MimeType { get; set; }
    }
}
