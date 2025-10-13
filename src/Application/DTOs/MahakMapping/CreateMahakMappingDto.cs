namespace OnlineShop.Application.DTOs.MahakMapping
{
    public class CreateMahakMappingDto
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid LocalEntityId { get; set; }
        public int MahakEntityId { get; set; }
        public string? MahakEntityCode { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateMahakMappingDto
    {
        public Guid Id { get; set; }
        public int MahakEntityId { get; set; }
        public string? MahakEntityCode { get; set; }
        public string? Notes { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class MahakMappingDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid LocalEntityId { get; set; }
        public int MahakEntityId { get; set; }
        public string? MahakEntityCode { get; set; }
        public string MappingStatus { get; set; } = string.Empty;
        public DateTime MappedAt { get; set; }
        public DateTime? UnmappedAt { get; set; }
        public string? UnmappedReason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

