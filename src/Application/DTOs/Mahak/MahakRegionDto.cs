namespace OnlineShop.Application.DTOs.Mahak
{
    public class MahakRegionDto
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public long RowVersion { get; set; }
    }
}
