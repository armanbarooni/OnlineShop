namespace OnlineShop.Application.DTOs.Season
{
    public class UpdateSeasonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

