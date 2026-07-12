using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Mahak;
using OnlineShop.Infrastructure.Mahak.Models;

namespace OnlineShop.Infrastructure.Services
{
    public class MahakRegionService : IMahakRegionService
    {
        private const string BaseUrl = "https://mahakacc.mahaksoft.com/API/v3/Sync/";
        private const string CacheKey = "MahakRegions";
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MahakRegionService> _logger;
        private readonly IMemoryCache _cache;

        public MahakRegionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<MahakRegionService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IReadOnlyList<MahakRegionDto>> GetRegionsAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CacheKey, out IReadOnlyList<MahakRegionDto>? cached) && cached != null)
            {
                return cached;
            }

            if (!IsConfigured())
            {
                _logger.LogWarning(
                    "Mahak regions requested but Mahak credentials are not configured. UsernameConfigured={UsernameConfigured}, PasswordConfigured={PasswordConfigured}",
                    !string.IsNullOrWhiteSpace(_configuration["Mahak:Username"]),
                    !string.IsNullOrWhiteSpace(_configuration["Mahak:Password"]));
                throw new InvalidOperationException("Mahak credentials are not configured.");
            }

            var token = await LoginAsync(cancellationToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new RequestAllDataModel { FromRegionVersion = 0 };
            using var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json-patch+json");
            var response = await _httpClient.PostAsync("GetAllData", content, cancellationToken);
            var responseText = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync(cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Mahak regions request failed. Status={StatusCode}, Body={Body}",
                    response.StatusCode,
                    Truncate(responseText, 1000));
                throw new InvalidOperationException($"Mahak regions request failed. Status: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<MahakApiResult<GetAllDataResponse>>(responseText, JsonOptions);
            if (result == null || !result.Result)
            {
                throw new InvalidOperationException($"Mahak regions request was rejected. Message: {result?.Message ?? responseText}");
            }

            var regions = result.Data?.Objects?.Regions?
                .Where(region => region.CityID > 0 && !string.IsNullOrWhiteSpace(region.CityName) && !string.IsNullOrWhiteSpace(region.ProvinceName))
                .Select(region => new MahakRegionDto
                {
                    CityId = region.CityID,
                    CityName = NormalizePersianText(region.CityName),
                    ProvinceId = region.ProvinceID,
                    ProvinceName = NormalizePersianText(region.ProvinceName),
                    RowVersion = region.RowVersion
                })
                .Where(region =>
                    !string.IsNullOrWhiteSpace(region.CityName) &&
                    !string.IsNullOrWhiteSpace(region.ProvinceName) &&
                    !region.CityName.StartsWith("استان ", StringComparison.OrdinalIgnoreCase))
                .OrderBy(region => region.ProvinceName)
                .ThenBy(region => region.CityName)
                .ToList() ?? new List<MahakRegionDto>();

            _logger.LogInformation(
                "Mahak regions loaded. Count={Count}, ResponsePreview={ResponsePreview}",
                regions.Count,
                Truncate(responseText, 500));

            if (regions.Count > 0)
            {
                _cache.Set(CacheKey, regions, TimeSpan.FromHours(12));
            }

            return regions;
        }

        private bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(_configuration["Mahak:Username"]) &&
                   !string.IsNullOrWhiteSpace(_configuration["Mahak:Password"]);
        }

        private async Task<string> LoginAsync(CancellationToken cancellationToken)
        {
            var username = _configuration["Mahak:Username"];
            var password = _configuration["Mahak:Password"];
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Mahak configuration is incomplete.");
            }

            using var md5 = MD5.Create();
            var hashedPassword = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
            var loginModel = new { userName = username, password = hashedPassword };

            using var content = new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json-patch+json");
            var response = await _httpClient.PostAsync("Login", content, cancellationToken);
            var responseText = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync(cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Mahak login failed. Status: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<MahakApiResult<LoginResultModel>>(responseText, JsonOptions);
            if (result == null || !result.Result || string.IsNullOrWhiteSpace(result.Data?.UserToken))
            {
                throw new InvalidOperationException($"Mahak login failed. Message: {result?.Message ?? responseText}");
            }

            return result.Data.UserToken;
        }

        private static string NormalizePersianText(string? value)
        {
            return (value ?? string.Empty)
                .Replace('\u064A', '\u06CC')
                .Replace('\u0643', '\u06A9')
                .Trim();
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength];
        }
    }
}
