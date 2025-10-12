using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Application.DTOs.SavedCart
{
    public class SavedCartDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CartId { get; set; }
        public string SavedCartName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime SavedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public int AccessCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
