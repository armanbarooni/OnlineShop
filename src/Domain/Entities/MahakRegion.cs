using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class MahakRegion : BaseEntity
    {
        private MahakRegion() { }

        private MahakRegion(int cityId, string cityName, int provinceId, string provinceName, string? mapCode, long mahakRowVersion)
        {
            CityId = cityId;
            ProvinceId = provinceId;
            MapCode = mapCode;
            SetNames(cityName, provinceName);
            SetMahakRowVersion(mahakRowVersion);
        }

        public int CityId { get; private set; }
        public string CityName { get; private set; } = string.Empty;
        public int ProvinceId { get; private set; }
        public string ProvinceName { get; private set; } = string.Empty;
        public string? MapCode { get; private set; }
        public long MahakRowVersion { get; private set; }
        public DateTime SyncedAt { get; private set; } = DateTime.UtcNow;

        public static MahakRegion Create(int cityId, string cityName, int provinceId, string provinceName, string? mapCode, long mahakRowVersion)
        {
            if (cityId <= 0) throw new ArgumentException("City id must be positive.", nameof(cityId));
            if (provinceId <= 0) throw new ArgumentException("Province id must be positive.", nameof(provinceId));

            return new MahakRegion(cityId, cityName, provinceId, provinceName, mapCode, mahakRowVersion);
        }

        public void Update(string cityName, int provinceId, string provinceName, string? mapCode, long mahakRowVersion)
        {
            if (provinceId <= 0) throw new ArgumentException("Province id must be positive.", nameof(provinceId));

            ProvinceId = provinceId;
            MapCode = string.IsNullOrWhiteSpace(mapCode) ? null : mapCode.Trim();
            SetNames(cityName, provinceName);
            SetMahakRowVersion(mahakRowVersion);
            SyncedAt = DateTime.UtcNow;
            Deleted = false;
        }

        private void SetNames(string cityName, string provinceName)
        {
            if (string.IsNullOrWhiteSpace(cityName)) throw new ArgumentException("City name is required.", nameof(cityName));
            if (string.IsNullOrWhiteSpace(provinceName)) throw new ArgumentException("Province name is required.", nameof(provinceName));

            CityName = cityName.Trim();
            ProvinceName = provinceName.Trim();
        }

        private void SetMahakRowVersion(long mahakRowVersion)
        {
            MahakRowVersion = Math.Max(mahakRowVersion, 0);
        }
    }
}
