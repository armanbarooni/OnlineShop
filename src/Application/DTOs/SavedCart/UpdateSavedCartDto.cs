using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.SavedCart
{
    public class UpdateSavedCartDto
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SavedCartName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsFavorite { get; set; }
    }
}
