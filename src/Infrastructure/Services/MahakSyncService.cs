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
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IMahakMappingRepository _mahakMappingRepository;

        private string _token;
        private const string BaseUrl = "https://mahakacc.mahaksoft.com/API/v3/Sync/";

        public MahakSyncService(
            HttpClient httpClient,
            ILogger<MahakSyncService> logger,
            IConfiguration configuration,
            IMahakSyncLogRepository mahakSyncLogRepository,
            IProductRepository productRepository,
            IProductCategoryRepository productCategoryRepository,
            IMahakMappingRepository mahakMappingRepository)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _mahakSyncLogRepository = mahakSyncLogRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _mahakMappingRepository = mahakMappingRepository;
            
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
                long fromProductDetailVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductDetail", cancellationToken);
                long fromProductCategoryVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductCategory", cancellationToken);
                long fromPictureVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("Picture", cancellationToken);
                long fromPhotoGalleryVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("PhotoGallery", cancellationToken);
                long fromProductDetailStoreAssetVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductDetailStoreAsset", cancellationToken);
                
                _logger.LogInformation("Requesting data from Mahak with RowVersions - Product: {ProductVer}, ProductDetail: {DetailVer}, Category: {CategoryVer}", 
                    fromProductVersion, fromProductDetailVersion, fromProductCategoryVersion);
                
                // 3. Request Data
                var request = new RequestAllDataModel
                {
                    FromProductVersion = fromProductVersion,
                    FromProductDetailVersion = fromProductDetailVersion,
                    FromProductCategoryVersion = fromProductCategoryVersion,
                    FromPictureVersion = fromPictureVersion,
                    FromPhotoGalleryVersion = fromPhotoGalleryVersion,
                    FromProductDetailStoreAssetVersion = fromProductDetailStoreAssetVersion
                };

                var response = await GetAllDataAsync(request, cancellationToken);
                
                if (response?.Objects == null)
                {
                    _logger.LogInformation("No data received from Mahak.");
                    return;
                }

                // 4. Process Data (Order matters: Categories → Products → ProductDetails → Inventory → Images)
                await ProcessCategoriesAsync(response.Objects.ProductCategories, cancellationToken);
                await ProcessProductsAsync(response.Objects.Products, cancellationToken);
                await ProcessProductDetailsAsync(response.Objects.ProductDetails, cancellationToken);
                await ProcessInventoryAsync(response.Objects.ProductDetailStoreAssets, cancellationToken);
                await ProcessImagesAsync(response.Objects.PhotoGalleries, response.Objects.Pictures, cancellationToken);

                // 5. Log Success (Updating local RowVersions happens via storing MahakSyncLog entries)
                if (response.Objects.ProductCategories != null && response.Objects.ProductCategories.Any())
                {
                     var maxVersion = response.Objects.ProductCategories.Max(x => x.RowVersion);
                     await LogSyncAsync("ProductCategory", maxVersion, response.Objects.ProductCategories.Count, "Success", cancellationToken);
                }

                if (response.Objects.Products != null && response.Objects.Products.Any())
                {
                     var maxVersion = response.Objects.Products.Max(x => x.RowVersion);
                     await LogSyncAsync("Product", maxVersion, response.Objects.Products.Count, "Success", cancellationToken);
                }

                if (response.Objects.ProductDetails != null && response.Objects.ProductDetails.Any())
                {
                     var maxVersion = response.Objects.ProductDetails.Max(x => x.RowVersion);
                     await LogSyncAsync("ProductDetail", maxVersion, response.Objects.ProductDetails.Count, "Success", cancellationToken);
                }

                if (response.Objects.ProductDetailStoreAssets != null && response.Objects.ProductDetailStoreAssets.Any())
                {
                     var maxVersion = response.Objects.ProductDetailStoreAssets.Max(x => x.RowVersion);
                     await LogSyncAsync("ProductDetailStoreAsset", maxVersion, response.Objects.ProductDetailStoreAssets.Count, "Success", cancellationToken);
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

            var loginModel = new
            {
                userName = username,
                password = hashedPassword
            };

            _logger.LogInformation("Attempting Mahak login for user: {Username}, MD5 Hash: {Hash}", username, hashedPassword);

            var content = new StringContent(JsonSerializer.Serialize(loginModel), System.Text.Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");
            
            var response = await _httpClient.PostAsync("Login", content, cancellationToken); // Use simple Login endpoint
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Mahak login failed. Status: {Status}, Content: {Content}", response.StatusCode, errorContent);
                throw new Exception($"Login failed. Status: {response.StatusCode}, Content: {errorContent}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Login response: {Response}", responseText);

            var result = await JsonSerializer.DeserializeAsync<MahakApiResult<LoginResultModel>>(
                await response.Content.ReadAsStreamAsync(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, 
                cancellationToken);

            if (result == null || !result.Result || result.Data == null)
            {
                 _logger.LogError("Mahak login returned invalid result: {Message}", result?.Message);
                 throw new Exception($"Login failed. Message: {result?.Message}");
            }

            _token = result.Data.UserToken; // Use UserToken from response
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            
            _logger.LogInformation("Mahak login successful. Token received (length: {Length})", _token?.Length ?? 0);
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
            if (products == null || !products.Any())
            {
                _logger.LogInformation("No products to process from Mahak");
                return;
            }
            
            _logger.LogInformation("Processing {Count} products from Mahak", products.Count);
            
            int created = 0;
            int updated = 0;
            int errors = 0;

            foreach (var mahakProduct in products)
            {
                try
                {
                    // Skip deleted products
                    if (mahakProduct.Deleted)
                    {
                        _logger.LogDebug("Skipping deleted product: {ProductId}", mahakProduct.ProductId);
                        continue;
                    }

                    // Check if mapping exists
                    var mapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "Product", 
                        mahakProduct.ProductId, 
                        cancellationToken);

                    Product? existingProduct = null;
                    
                    if (mapping != null)
                    {
                        // Get existing product by mapping
                        existingProduct = await _productRepository.GetByIdAsync(
                            mapping.LocalEntityId, 
                            cancellationToken);
                    }

                    if (existingProduct != null)
                    {
                        // UPDATE existing product
                        _logger.LogDebug("Updating product: {Name} (MahakId: {MahakId})", 
                            mahakProduct.Name, mahakProduct.ProductId);

                        existingProduct.SetName(mahakProduct.Name);
                        existingProduct.SetDescription(mahakProduct.Description ?? "");
                        
                        // Note: Price and stock will be updated from ProductDetail and ProductDetailStoreAsset
                        // For now, we just update basic info
                        
                        await _productRepository.UpdateAsync(existingProduct, cancellationToken);
                        updated++;
                    }
                    else
                    {
                        // CREATE new product
                        _logger.LogDebug("Creating new product: {Name} (MahakId: {MahakId})", 
                            mahakProduct.Name, mahakProduct.ProductId);

                        var newProduct = Product.Create(
                            name: mahakProduct.Name,
                            description: mahakProduct.Description ?? "",
                            price: 0, // Will be updated from ProductDetail
                            stockQuantity: 0, // Will be updated from ProductDetailStoreAsset
                            mahakClientId: mahakProduct.ProductClientId,
                            mahakId: mahakProduct.ProductId
                        );

                        // Set additional properties
                        if (!string.IsNullOrEmpty(mahakProduct.UnitName))
                        {
                            // TODO: Map unit name to UnitId
                            // For now, we'll skip this
                        }

                        await _productRepository.AddAsync(newProduct, cancellationToken);

                        // Create mapping
                        var newMapping = MahakMapping.Create(
                            entityType: "Product",
                            localEntityId: newProduct.Id,
                            mahakEntityId: mahakProduct.ProductId,
                            mahakEntityCode: mahakProduct.ProductCode.ToString(),
                            notes: $"Synced from Mahak on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                        );

                        await _mahakMappingRepository.AddAsync(newMapping, cancellationToken);
                        created++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing product {ProductId}: {Error}", 
                        mahakProduct.ProductId, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "Product sync completed: {Created} created, {Updated} updated, {Errors} errors", 
                created, updated, errors);
        }

        private async Task ProcessCategoriesAsync(List<ProductCategoryModel>? categories, CancellationToken cancellationToken)
        {
            if (categories == null || !categories.Any())
            {
                _logger.LogInformation("No categories to process from Mahak");
                return;
            }
            
            _logger.LogInformation("Processing {Count} categories from Mahak", categories.Count);
            
            int created = 0;
            int updated = 0;
            int errors = 0;

            foreach (var mahakCategory in categories)
            {
                try
                {
                    if (mahakCategory.Deleted)
                    {
                        _logger.LogDebug("Skipping deleted category: {CategoryId}", mahakCategory.ProductCategoryId);
                        continue;
                    }

                    var mapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "ProductCategory", 
                        mahakCategory.ProductCategoryId, 
                        cancellationToken);

                    ProductCategory? existingCategory = null;
                    
                    if (mapping != null)
                    {
                        existingCategory = await _productCategoryRepository.GetByIdAsync(
                            mapping.LocalEntityId, 
                            cancellationToken);
                    }

                    if (existingCategory != null)
                    {
                        _logger.LogDebug("Updating category: {Name} (MahakId: {MahakId})", 
                            mahakCategory.Name, mahakCategory.ProductCategoryId);

                        existingCategory.SetName(mahakCategory.Name ?? "");
                        existingCategory.SetDescription(mahakCategory.Name ?? "");
                        
                        await _productCategoryRepository.UpdateAsync(existingCategory, cancellationToken);
                        updated++;
                    }
                    else
                    {
                        _logger.LogDebug("Creating new category: {Name} (MahakId: {MahakId})", 
                            mahakCategory.Name, mahakCategory.ProductCategoryId);

                        var newCategory = ProductCategory.Create(
                            name: mahakCategory.Name ?? "Unknown Category",
                            description: mahakCategory.Name ?? "",
                            mahakClientId: mahakCategory.ProductCategoryClientId,
                            mahakId: mahakCategory.ProductCategoryId
                        );

                        await _productCategoryRepository.AddAsync(newCategory, cancellationToken);

                        var newMapping = MahakMapping.Create(
                            entityType: "ProductCategory",
                            localEntityId: newCategory.Id,
                            mahakEntityId: mahakCategory.ProductCategoryId,
                            mahakEntityCode: mahakCategory.ProductCategoryCode.ToString(),
                            notes: $"Synced from Mahak on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                        );

                        await _mahakMappingRepository.AddAsync(newMapping, cancellationToken);
                        created++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing category {CategoryId}: {Error}", 
                        mahakCategory.ProductCategoryId, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "Category sync completed: {Created} created, {Updated} updated, {Errors} errors", 
                created, updated, errors);
        }

        private async Task ProcessProductDetailsAsync(List<ProductDetailModel>? productDetails, CancellationToken cancellationToken)
        {
            if (productDetails == null || !productDetails.Any())
            {
                _logger.LogInformation("No product details to process from Mahak");
                return;
            }
            
            _logger.LogInformation("Processing {Count} product details from Mahak", productDetails.Count);
            
            int updated = 0;
            int errors = 0;

            // Group by ProductId to avoid EF tracking conflicts (multiple ProductDetails per Product)
            var detailsByProduct = productDetails
                .Where(d => !d.Deleted)
                .GroupBy(d => d.ProductId)
                .ToList();

            foreach (var group in detailsByProduct)
            {
                try
                {
                    var firstDetail = group.First(); // Use first detail for price
                    
                    var productMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "Product", 
                        firstDetail.ProductId, 
                        cancellationToken);

                    if (productMapping == null)
                    {
                        _logger.LogWarning("Product not found for ProductId: {ProductId}", firstDetail.ProductId);
                        continue;
                    }

                    var product = await _productRepository.GetByIdAsync(productMapping.LocalEntityId, cancellationToken);
                    
                    if (product == null)
                    {
                        _logger.LogWarning("Product entity not found for mapping {MappingId}", productMapping.Id);
                        continue;
                    }

                    decimal price = firstDetail.DefaultSellPriceLevel switch
                    {
                        1 => firstDetail.Price1,
                        2 => firstDetail.Price2,
                        3 => firstDetail.Price3,
                        4 => firstDetail.Price4,
                        5 => firstDetail.Price5,
                        _ => firstDetail.Price1
                    };

                    if (price > 0)
                    {
                        product.SetPrice(price);
                        _logger.LogDebug("Updated price for product {ProductId}: {Price} ({Count} variants)", 
                            firstDetail.ProductId, price, group.Count());
                    }

                    if (!string.IsNullOrEmpty(firstDetail.Barcode))
                    {
                        product.SetBarcode(firstDetail.Barcode);
                    }

                    await _productRepository.UpdateAsync(product, cancellationToken);
                    updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing product details for ProductId {ProductId}: {Error}", 
                        group.Key, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "ProductDetail sync completed: {Updated} updated, {Errors} errors", 
                updated, errors);
        }

        private async Task ProcessInventoryAsync(List<ProductDetailStoreAssetModel>? inventory, CancellationToken cancellationToken)
        {
            if (inventory == null || !inventory.Any())
            {
                _logger.LogInformation("No inventory to process from Mahak");
                return;
            }
            
            _logger.LogInformation("Processing {Count} inventory records from Mahak", inventory.Count);
            
            int updated = 0;
            int errors = 0;

            var inventoryByProduct = inventory
                .Where(i => !i.Deleted)
                .GroupBy(i => i.ProductDetailId)
                .Select(g => new
                {
                    ProductDetailId = g.Key,
                    TotalQuantity = (int)g.Sum(i => i.Count1)
                })
                .ToList();

            foreach (var inv in inventoryByProduct)
            {
                try
                {
                    var productMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "Product", 
                        inv.ProductDetailId, 
                        cancellationToken);

                    if (productMapping == null)
                    {
                        _logger.LogDebug("Product not found for inventory ProductDetailId: {DetailId}", inv.ProductDetailId);
                        continue;
                    }

                    var product = await _productRepository.GetByIdAsync(productMapping.LocalEntityId, cancellationToken);
                    
                    if (product == null)
                    {
                        _logger.LogWarning("Product entity not found for mapping {MappingId}", productMapping.Id);
                        continue;
                    }

                    product.SetStockQuantity(inv.TotalQuantity);
                    _logger.LogDebug("Updated stock for product {ProductId}: {Quantity}", 
                        inv.ProductDetailId, inv.TotalQuantity);

                    await _productRepository.UpdateAsync(product, cancellationToken);
                    updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing inventory for ProductDetailId {DetailId}: {Error}", 
                        inv.ProductDetailId, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "Inventory sync completed: {Updated} updated, {Errors} errors", 
                updated, errors);
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
