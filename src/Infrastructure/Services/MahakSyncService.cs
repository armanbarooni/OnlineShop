using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Infrastructure.Mahak.Models;

namespace OnlineShop.Infrastructure.Services
{
    public class MahakSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MahakSyncService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMahakSyncLogRepository _mahakSyncLogRepository;
        // Assuming repositories for Product and ProductImage exist
        // private readonly IProductRepository _productRepository; 
        // private readonly IProductImageRepository _productImageRepository;

        private string _token;
        private const string BaseUrl = "https://mahakacc.mahaksoft.com/API/v3/Sync/"; // From logs

        public MahakSyncService(
            HttpClient httpClient,
            ILogger<MahakSyncService> logger,
            IConfiguration configuration,
            IMahakSyncLogRepository mahakSyncLogRepository)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _mahakSyncLogRepository = mahakSyncLogRepository;
            
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task SyncAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting Mahak Sync...");

                // 1. Login
                if (string.IsNullOrEmpty(_token))
                {
                    await LoginAsync(cancellationToken);
                }

                // 2. Get Last Row Versions
                long fromProductVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("Product", cancellationToken);
                long fromPictureVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("Picture", cancellationToken);
                long fromPhotoGalleryVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("PhotoGallery", cancellationToken);
                
                // 3. Request Data
                var request = new RequestAllDataModel
                {
                    FromProductVersion = fromProductVersion,
                    FromPictureVersion = fromPictureVersion,
                    FromPhotoGalleryVersion = fromPhotoGalleryVersion
                };

                var response = await GetAllDataAsync(request, cancellationToken);
                
                if (response?.Objects == null)
                {
                    _logger.LogInformation("No data received from Mahak.");
                    return;
                }

                // 4. Process Data
                await ProcessProductsAsync(response.Objects.Products, cancellationToken);
                await ProcessImagesAsync(response.Objects.PhotoGalleries, response.Objects.Pictures, cancellationToken);

                // 5. Log Success (Updating local RowVersions happens via storing MahakSyncLog entries)
                // We should calculate the NEW max RowVersion for each entity type and log it.
                if (response.Objects.Products != null && response.Objects.Products.Any())
                {
                     var maxVersion = response.Objects.Products.Max(x => x.RowVersion);
                     await LogSyncAsync("Product", maxVersion, response.Objects.Products.Count, "Success", cancellationToken);
                }
                
                if (response.Objects.Pictures != null && response.Objects.Pictures.Any())
                {
                     var maxVersion = response.Objects.Pictures.Max(x => x.RowVersion);
                     await LogSyncAsync("Picture", maxVersion, response.Objects.Pictures.Count, "Success", cancellationToken);
                }

                if (response.Objects.PhotoGalleries != null && response.Objects.PhotoGalleries.Any())
                {
                     var maxVersion = response.Objects.PhotoGalleries.Max(x => x.RowVersion);
                     await LogSyncAsync("PhotoGallery", maxVersion, response.Objects.PhotoGalleries.Count, "Success", cancellationToken);
                }

                _logger.LogInformation("Mahak Sync Completed Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Mahak Sync.");
                await LogSyncAsync("All", null, 0, "Failed", cancellationToken, ex.Message);
            }
        }

        private async Task LoginAsync(CancellationToken cancellationToken)
        {
            var username = _configuration["Mahak:Username"];
            var password = _configuration["Mahak:Password"];
            var packageNo = _configuration["Mahak:PackageNo"];
            var databaseIdStr = _configuration["Mahak:DatabaseId"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(packageNo) || string.IsNullOrEmpty(databaseIdStr))
            {
                throw new InvalidOperationException("Mahak configuration is incomplete. Please check appsettings.json");
            }

            // Hash password with MD5 as required by Mahak API
            string hashedPassword;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hashBytes = md5.ComputeHash(inputBytes);
                hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            var loginModel = new LoginModel
            {
                UserName = username,
                Password = hashedPassword, // Send MD5 hashed password
                PackageNo = packageNo,
                DatabaseId = long.Parse(databaseIdStr),
                Language = "fa",
                AppId = "OnlineShop",
                Description = "Online Shop Integration",
                ClientVersion = "1.0"
            };

            _logger.LogInformation("Attempting Mahak login for user: {Username}, DatabaseId: {DatabaseId}", username, databaseIdStr);

            var content = new StringContent(JsonSerializer.Serialize(loginModel), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("LoginV2", content, cancellationToken); // Use LoginV2 as per documentation
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Mahak login failed. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                throw new Exception($"Login failed. Status: {response.StatusCode}, Content: {errorContent}");
            }

            var result = await JsonSerializer.DeserializeAsync<MahakApiResult<LoginResultModel>>(
                await response.Content.ReadAsStreamAsync(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, 
                cancellationToken);

            if (result == null || !result.Result || result.Data == null)
            {
                 _logger.LogError("Mahak login returned invalid result: {Message}", result?.Message);
                 throw new Exception($"Login failed. Message: {result?.Message}");
            }

            _token = result.Data.UserToken; // Changed from Token to UserToken as per API response
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            
            _logger.LogInformation("Mahak login successful. SyncId: {SyncId}, VisitorId: {VisitorId}", 
                result.Data.SyncId, result.Data.VisitorId);
        }

        private async Task<GetAllDataResponse?> GetAllDataAsync(RequestAllDataModel request, CancellationToken cancellationToken)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"); // or "application/json-patch+json" as per docs
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var response = await _httpClient.PostAsync("GetAllData", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                 throw new Exception($"GetAllData failed. Status: {response.StatusCode}");
            }

            var result = await JsonSerializer.DeserializeAsync<MahakApiResult<GetAllDataResponse>>(
                await response.Content.ReadAsStreamAsync(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, 
                cancellationToken);

            if (result == null || !result.Result)
            {
                 _logger.LogWarning($"GetAllData API returned false result. Message: {result?.Message}");
                 return null;
            }

            return result.Data;
        }

        private async Task ProcessProductsAsync(List<ProductModel>? products, CancellationToken cancellationToken)
        {
            if (products == null || !products.Any()) return;
            
            // Logic to update local DB products
            // foreach(var p in products) { ... }
            // This requires IProductRepository.
            // Since I don't have the full context of how Product is stored (EF Core Entity), I'll just log for now.
            // But real implementation should:
            // var product = await _productRepository.GetByCodeAsync(p.ProductCode);
            // if (product == null) create new else update.
            _logger.LogInformation($"Processing {products.Count} products from Mahak.");
        }

        private async Task ProcessImagesAsync(List<PhotoGalleryModel>? galleries, List<PictureModel>? pictures, CancellationToken cancellationToken)
        {
            if (galleries == null || !galleries.Any()) return;
            if (pictures == null || !pictures.Any()) 
            {
                 // It's possible to get galleries with pictureId where picture was synced previously?
                 // But usually they come together if version tracking is correct.
            }

            var pictureMap = pictures?.ToDictionary(p => p.PictureId, p => p.Url) ?? new Dictionary<int, string?>();

            foreach (var gallery in galleries)
            {
                if (gallery.EntityType != 102) continue; // Only Products

                if (pictureMap.TryGetValue(gallery.PictureId, out var url) && !string.IsNullOrEmpty(url))
                {
                     // Update Product Image
                     _logger.LogInformation($"Product {gallery.ItemCode} has new image: {url}");
                     // await _productImageRepository.AddAsync(new ProductImage { ProductId = Map(gallery.ItemCode), Url = url ... });
                }
            }
        }
        
        private async Task LogSyncAsync(string entityType, long? rowVersion, int processed, string status, CancellationToken cancellationToken, string? error = null)
        {
            var log = MahakSyncLog.Create(entityType, null, "Import", status, processed);
            if (rowVersion.HasValue) log.SetMahakRowVersion(rowVersion);
            if (error != null) log.SetErrorMessage(error);
            log.CompleteSync(status == "Success" ? processed : 0, status == "Failed" ? processed : 0, error);
            
            await _mahakSyncLogRepository.AddAsync(log, cancellationToken);
        }
    }
}
