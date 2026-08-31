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
using Microsoft.AspNetCore.Identity;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Infrastructure.Mahak.Models;
using System.Text.Json.Serialization;

namespace OnlineShop.Infrastructure.Services
{
    public class MahakSyncService : IMahakInventorySyncService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MahakSyncService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMahakSyncLogRepository _mahakSyncLogRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IMahakMappingRepository _mahakMappingRepository;
        private readonly IProductDetailRepository _productDetailRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMahakTrafficLogger _mahakTrafficLogger;

        private string _token;
        private static readonly SemaphoreSlim TokenSemaphore = new(1, 1);
        private static string? _sharedToken;
        private static DateTimeOffset _sharedTokenExpiresAt = DateTimeOffset.MinValue;
        private const string BaseUrl = "https://mahakacc.mahaksoft.com/API/v3/Sync/";
        private const string MahakPictureMappingType = "Picture";
        private const string MahakPhotoGalleryMappingType = "PhotoGallery";

        public MahakSyncService(
            HttpClient httpClient,
            ILogger<MahakSyncService> logger,
            IConfiguration configuration,
            IMahakSyncLogRepository mahakSyncLogRepository,
            IProductRepository productRepository,
            IProductCategoryRepository productCategoryRepository,
            IMahakMappingRepository mahakMappingRepository,
            IProductDetailRepository productDetailRepository,
            IProductImageRepository productImageRepository,
            IProductVariantRepository productVariantRepository,
            UserManager<ApplicationUser> userManager,
            IMahakTrafficLogger mahakTrafficLogger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _mahakSyncLogRepository = mahakSyncLogRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _mahakMappingRepository = mahakMappingRepository;
            _productDetailRepository = productDetailRepository;
            _productImageRepository = productImageRepository;
            _productVariantRepository = productVariantRepository;
            _userManager = userManager;
            _mahakTrafficLogger = mahakTrafficLogger;
            
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (HasValidSharedToken())
            {
                ApplyAuthorizationHeader(_sharedToken!);
            }
        }

        /// <summary>
        /// Check if Mahak credentials are configured.
        /// </summary>
        public bool IsConfigured()
        {
            var username = _configuration["Mahak:Username"];
            var password = _configuration["Mahak:Password"];
            var packageNo = _configuration["Mahak:PackageNo"];
            var databaseId = _configuration["Mahak:DatabaseId"];
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) &&
                   !string.IsNullOrEmpty(packageNo) && !string.IsNullOrEmpty(databaseId);
        }

        public Task SyncInventoryFromMahakAsync(CancellationToken cancellationToken)
            => SyncAsync(cancellationToken);

        public async Task SyncAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Skip silently if Mahak is not configured
                if (!IsConfigured())
                {
                    _logger.LogDebug("Mahak incoming sync skipped: credentials not configured in appsettings.json");
                    return;
                }

                _logger.LogInformation("Starting Mahak Sync...");

                // 1. Login
                await EnsureAuthenticatedAsync(cancellationToken);

                // 2. Get Last Row Versions
                long fromProductVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("Product", cancellationToken);
                long fromProductDetailVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductDetail", cancellationToken);
                long fromProductCategoryVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductCategory", cancellationToken);
                long fromPictureVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("Picture", cancellationToken);
                long fromPhotoGalleryVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("PhotoGallery", cancellationToken);
                long fromProductDetailStoreAssetVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("ProductDetailStoreAsset", cancellationToken);
                long fromPersonVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("Person", cancellationToken);
                long fromVisitorPersonVersion = await _mahakSyncLogRepository.GetLastRowVersionAsync("VisitorPerson", cancellationToken);
                
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
                    FromProductDetailStoreAssetVersion = fromProductDetailStoreAssetVersion,
                    FromPersonVersion = fromPersonVersion,
                    FromVisitorPersonVersion = fromVisitorPersonVersion
                };

                var response = await GetAllDataAsync(request, cancellationToken);
                
                if (response?.Objects == null)
                {
                    _logger.LogInformation("No data received from Mahak.");
                    return;
                }

                // 4. Process Data (Order matters: Categories → Products → ProductDetails → Inventory → Images → People)
                await ProcessCategoriesAsync(response.Objects.ProductCategories, cancellationToken);
                await ProcessProductsAsync(response.Objects.Products, cancellationToken);
                var deletedProductDetailIds = await ProcessProductDetailsAsync(response.Objects.ProductDetails, cancellationToken);
                await ProcessInventoryAsync(response.Objects.ProductDetailStoreAssets, deletedProductDetailIds, cancellationToken);
                await ProcessImagesAsync(response.Objects.PhotoGalleries, response.Objects.Pictures, cancellationToken);
                await ProcessPeopleAsync(response.Objects.People, cancellationToken);
                ProcessVisitorPeople(response.Objects.VisitorPeople);

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

                if (response.Objects.VisitorPeople != null && response.Objects.VisitorPeople.Any())
                {
                     var maxVersion = response.Objects.VisitorPeople.Max(x => x.RowVersion);
                     await LogSyncAsync("VisitorPerson", maxVersion, response.Objects.VisitorPeople.Count, "Success", cancellationToken);
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

            await _mahakTrafficLogger.LogRequestAsync(
                "Login",
                "Login",
                "این دیتا برای ورود به محک و گرفتن توکن است",
                loginModel,
                cancellationToken);

            var content = new StringContent(JsonSerializer.Serialize(loginModel), System.Text.Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");
            
            var response = await _httpClient.PostAsync("Login", content, cancellationToken); // Use simple Login endpoint
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            await _mahakTrafficLogger.LogResponseAsync(
                "Login",
                "Login",
                "این دیتا پاسخ محک برای ورود و توکن است",
                (int)response.StatusCode,
                response.IsSuccessStatusCode,
                responseText,
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Mahak login failed. Status: {Status}, Content: {Content}", response.StatusCode, responseText);
                throw new Exception($"Login failed. Status: {response.StatusCode}, Content: {responseText}");
            }

            _logger.LogDebug("Login response: {Response}", responseText);

            var result = JsonSerializer.Deserialize<MahakApiResult<LoginResultModel>>(
                responseText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || !result.Result || result.Data == null)
            {
                 _logger.LogError("Mahak login returned invalid result: {Message}", result?.Message);
                 throw new Exception($"Login failed. Message: {result?.Message}");
            }

            _token = result.Data.UserToken; // Use UserToken from response
            _sharedToken = _token;
            _sharedTokenExpiresAt = ResolveSharedTokenExpiration();
            ApplyAuthorizationHeader(_token);
            
            _logger.LogInformation("Mahak login successful. Token received (length: {Length})", _token?.Length ?? 0);
        }

        private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
        {
            if (HasValidSharedToken())
            {
                ApplyAuthorizationHeader(_sharedToken!);
                return;
            }

            await TokenSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (!HasValidSharedToken())
                {
                    await LoginAsync(cancellationToken);
                }
                else
                {
                    ApplyAuthorizationHeader(_sharedToken!);
                }
            }
            finally
            {
                TokenSemaphore.Release();
            }
        }

        private bool HasValidSharedToken()
        {
            return !string.IsNullOrWhiteSpace(_sharedToken) &&
                   _sharedTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1);
        }

        private void ApplyAuthorizationHeader(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private DateTimeOffset ResolveSharedTokenExpiration()
        {
            var tokenCacheMinutes = _configuration.GetValue<int?>("Mahak:TokenCacheMinutes") ?? 20;
            tokenCacheMinutes = Math.Clamp(tokenCacheMinutes, 1, 120);
            return DateTimeOffset.UtcNow.AddMinutes(tokenCacheMinutes);
        }

        private async Task<GetAllDataResponse?> GetAllDataAsync(RequestAllDataModel request, CancellationToken cancellationToken)
        {
            await _mahakTrafficLogger.LogRequestAsync(
                "GetAllData",
                "GetAllData",
                "این دیتا برای گرفتن موجودی، کالا، دسته‌بندی، تصاویر و مشتری‌ها از محک است",
                request,
                cancellationToken);

            var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"); // or "application/json-patch+json" as per docs
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var response = await _httpClient.PostAsync("GetAllData", content, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            await _mahakTrafficLogger.LogResponseAsync(
                "GetAllData",
                "GetAllData",
                "این دیتا پاسخ محک برای موجودی، کالا، دسته‌بندی، تصاویر و مشتری‌ها است",
                (int)response.StatusCode,
                response.IsSuccessStatusCode,
                responseText,
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                 throw new Exception($"GetAllData failed. Status: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<MahakApiResult<GetAllDataResponse>>(
                responseText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
            int deleted = 0;
            int errors = 0;

            var latestProducts = products
                .GroupBy(product => product.ProductId)
                .Select(group => group.OrderByDescending(product => product.RowVersion).First())
                .ToList();

            foreach (var mahakProduct in latestProducts)
            {
                try
                {
                    var mapping = await _mahakMappingRepository.GetByMahakEntityIdIgnoreStatusAsync(
                        "Product",
                        mahakProduct.ProductId,
                        cancellationToken);
                    if (mapping != null &&
                        TryGetStoredProductRowVersion(mapping.Notes, out var storedProductRowVersion) &&
                        storedProductRowVersion > mahakProduct.RowVersion)
                    {
                        _logger.LogWarning(
                            "Skipping stale Mahak product payload: ProductId={ProductId}, IncomingRowVersion={IncomingRowVersion}, StoredRowVersion={StoredRowVersion}",
                            mahakProduct.ProductId,
                            mahakProduct.RowVersion,
                            storedProductRowVersion);
                        continue;
                    }

                    // Delete local product when Mahak marks it as deleted
                    if (mahakProduct.Deleted)
                    {
                        Product? localProduct = null;
                        if (mapping != null)
                        {
                            localProduct = await _productRepository.GetByIdIgnoreFiltersAsync(
                                mapping.LocalEntityId,
                                cancellationToken);
                        }

                        localProduct ??= await _productRepository.GetByMahakIdIgnoreFiltersAsync(
                            mahakProduct.ProductId,
                            cancellationToken);

                        if (localProduct != null)
                        {
                            if (!localProduct.DeletedByMahak)
                            {
                                localProduct.SetDeletedByMahak(true);
                                await _productRepository.UpdateAsync(localProduct, cancellationToken);
                                deleted++;
                            }

                            _logger.LogInformation(
                                "Marked local product as DeletedByMahak: MahakId={MahakId}, LocalId={LocalId}",
                                mahakProduct.ProductId,
                                localProduct.Id);
                        }
                        else
                        {
                            _logger.LogDebug(
                                "Deleted product from Mahak has no local mapping/local match: {ProductId}",
                                mahakProduct.ProductId);
                        }

                        if (mapping != null)
                        {
                            mapping.Update(
                                mahakProduct.ProductId,
                                mahakProduct.ProductCode.ToString(),
                                BuildProductSyncNotes(mahakProduct.RowVersion, "Deleted by Mahak"),
                                "MahakSync");
                            await _mahakMappingRepository.UpdateAsync(mapping, cancellationToken);
                        }

                        continue;
                    }

                    Product? existingProduct = null;
                    
                    if (mapping != null)
                    {
                        // Get existing product by mapping
                        existingProduct = await _productRepository.GetByIdIgnoreFiltersAsync(
                            mapping.LocalEntityId, 
                            cancellationToken);

                        // A stale mapping must not rename an unrelated local product.
                        // ProductId is the stable Mahak identity; ProductCode is not unique enough.
                        if (existingProduct != null &&
                            existingProduct.MahakId.HasValue &&
                            existingProduct.MahakId.Value != mahakProduct.ProductId)
                        {
                            _logger.LogWarning(
                                "Ignoring stale Mahak product mapping: MahakId={MahakId}, MappingLocalId={MappingLocalId}, ActualLocalMahakId={ActualLocalMahakId}",
                                mahakProduct.ProductId,
                                mapping.LocalEntityId,
                                existingProduct.MahakId);
                            existingProduct = null;
                        }
                    }

                    // Always recover by the immutable Mahak ProductId when the mapping is missing or stale.
                    if (existingProduct == null)
                    {
                        existingProduct = await _productRepository.GetByMahakIdIgnoreFiltersAsync(
                            mahakProduct.ProductId,
                            cancellationToken);

                        if (existingProduct != null)
                        {
                            if (mapping != null && mapping.LocalEntityId != existingProduct.Id)
                            {
                                mapping.SetLocalEntityId(existingProduct.Id);
                                mapping.Update(
                                    mahakProduct.ProductId,
                                    mahakProduct.ProductCode.ToString(),
                                    $"Repaired stale mapping from MahakId on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                                    "MahakSync");
                                await _mahakMappingRepository.UpdateAsync(mapping, cancellationToken);
                            }
                            else if (mapping == null)
                            {
                                mapping = MahakMapping.Create(
                                    entityType: "Product",
                                    localEntityId: existingProduct.Id,
                                    mahakEntityId: mahakProduct.ProductId,
                                    mahakEntityCode: mahakProduct.ProductCode.ToString(),
                                    notes: BuildProductSyncNotes(mahakProduct.RowVersion, "Recovered mapping from MahakId"));

                                await _mahakMappingRepository.AddAsync(mapping, cancellationToken);
                            }
                        }
                        else if (mapping != null)
                        {
                            // Remove an orphan mapping before creating the correct product mapping.
                            await _mahakMappingRepository.DeleteAsync(mapping.Id, cancellationToken);
                            mapping = null;
                        }
                    }

                    if (existingProduct != null)
                    {
                        var localUpdatedAt = existingProduct.UpdatedAt ?? existingProduct.LastModifiedAt ?? existingProduct.CreatedAt;
                        if (localUpdatedAt > DateTime.UtcNow)
                        {
                            _logger.LogInformation(
                                "Skipping product update from Mahak because local product is newer. MahakId={MahakId}, LocalId={LocalId}, LocalUpdatedAt={LocalUpdatedAt}",
                                mahakProduct.ProductId,
                                existingProduct.Id,
                                localUpdatedAt);
                            continue;
                        }

                        // UPDATE existing product
                        _logger.LogDebug("Updating product: {Name} (MahakId: {MahakId})", 
                            mahakProduct.Name, mahakProduct.ProductId);

                        existingProduct.SetName(mahakProduct.Name);
                        existingProduct.SetDescription(mahakProduct.Description ?? "");
                        existingProduct.MahakId = mahakProduct.ProductId;
                        existingProduct.MahakClientId = mahakProduct.ProductClientId;
                        if (existingProduct.DeletedByMahak)
                        {
                            existingProduct.SetDeletedByMahak(false);
                        }

                        // Link product to category when the Mahak category mapping exists.
                        if (mahakProduct.ProductCategoryId > 0)
                        {
                            var categoryMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                                "ProductCategory",
                                mahakProduct.ProductCategoryId,
                                cancellationToken);

                            if (categoryMapping != null)
                            {
                                existingProduct.SetCategoryId(categoryMapping.LocalEntityId);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Category mapping not found for Mahak ProductCategoryId {CategoryId} while updating product {ProductId}",
                                    mahakProduct.ProductCategoryId,
                                    mahakProduct.ProductId);
                            }
                        }
                        
                        // Note: Price and stock will be updated from ProductDetail and ProductDetailStoreAsset
                        // For now, we just update basic info
                        
                        await _productRepository.UpdateAsync(existingProduct, cancellationToken);
                        if (mapping != null)
                        {
                            mapping.Update(
                                mahakProduct.ProductId,
                                mahakProduct.ProductCode.ToString(),
                                BuildProductSyncNotes(mahakProduct.RowVersion, "Updated from Mahak"),
                                "MahakSync");
                            mapping.Reactivate(mapping.Notes);
                            await _mahakMappingRepository.UpdateAsync(mapping, cancellationToken);
                        }
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

                        // Link product to category when the Mahak category mapping exists.
                        if (mahakProduct.ProductCategoryId > 0)
                        {
                            var categoryMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                                "ProductCategory",
                                mahakProduct.ProductCategoryId,
                                cancellationToken);

                            if (categoryMapping != null)
                            {
                                newProduct.SetCategoryId(categoryMapping.LocalEntityId);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Category mapping not found for Mahak ProductCategoryId {CategoryId} while creating product {ProductId}",
                                    mahakProduct.ProductCategoryId,
                                    mahakProduct.ProductId);
                            }
                        }

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
                            notes: BuildProductSyncNotes(mahakProduct.RowVersion, "Created from Mahak")
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
                "Product sync completed: {Created} created, {Updated} updated, {Deleted} deleted, {Errors} errors",
                created, updated, deleted, errors);
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

        private async Task<HashSet<int>> ProcessProductDetailsAsync(List<ProductDetailModel>? productDetails, CancellationToken cancellationToken)
        {
            if (productDetails == null || !productDetails.Any())
            {
                _logger.LogInformation("No product details to process from Mahak");
                return new HashSet<int>();
            }
            
            _logger.LogInformation("Processing {Count} product details from Mahak", productDetails.Count);
            
            int updated = 0;
            int errors = 0;

            var latestDetails = productDetails
                .GroupBy(d => d.ProductDetailId)
                .Select(group => group.OrderByDescending(d => d.RowVersion).First())
                .ToList();

            var deletedProductDetailIds = latestDetails
                .Where(d => d.Deleted)
                .Select(d => d.ProductDetailId)
                .ToHashSet();

            if (latestDetails.Count != productDetails.Count)
            {
                _logger.LogInformation(
                    "Collapsed {OriginalCount} ProductDetail rows to {LatestCount} latest rows by ProductDetailId",
                    productDetails.Count,
                    latestDetails.Count);
            }

            // Group by ProductId to avoid EF tracking conflicts (multiple ProductDetails per Product)
            var detailsByProduct = latestDetails
                .GroupBy(d => d.ProductId)
                .ToList();

            foreach (var group in detailsByProduct)
            {
                try
                {
                    var firstDetail = group.First(); // Use first detail for resolving product mapping
                    var firstActiveDetail = group
                        .Where(d => !d.Deleted)
                        .OrderByDescending(d => d.RowVersion)
                        .FirstOrDefault();
                    
                    var productMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "Product", 
                        firstDetail.ProductId, 
                        cancellationToken);

                    if (productMapping == null)
                    {
                        _logger.LogWarning("Product not found for ProductId: {ProductId}", firstDetail.ProductId);
                        continue;
                    }

                    var product = await _productRepository.GetByIdTrackedAsync(productMapping.LocalEntityId, cancellationToken);
                    
                    if (product == null)
                    {
                        _logger.LogWarning("Product entity not found for mapping {MappingId}", productMapping.Id);
                        continue;
                    }

                    if (firstActiveDetail != null)
                    {
                        if (firstActiveDetail.Price1 > 0)
                        {
                            product.SetPrice(firstActiveDetail.Price1);
                        }

                        product.SetPrice2(firstActiveDetail.Price2);
                        _logger.LogDebug("Updated prices for product {ProductId}: Price1={Price1}, Price2={Price2} ({Count} variants)",
                            firstDetail.ProductId, firstActiveDetail.Price1, firstActiveDetail.Price2, group.Count());

                        if (!string.IsNullOrEmpty(firstActiveDetail.Barcode))
                        {
                            product.SetBarcode(firstActiveDetail.Barcode);
                        }

                        await _productRepository.UpdateAsync(product, cancellationToken);
                    }
                    
                    // Create/update mappings for ProductDetail and ProductVariant (color/size)
                    foreach (var detail in group)
                    {
                        await UpsertProductDetailFromMahakDetail(detail, product, cancellationToken);

                        if (detail.Deleted)
                        {
                            await DeleteVariantFromDetail(detail, cancellationToken);
                            continue;
                        }

                        await EnsureProductDetailMapping(detail, product, cancellationToken);
                        await UpsertVariantFromDetail(detail, product, cancellationToken);
                    }
                    
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
                "ProductDetail sync completed: {Updated} updated, {Deleted} deleted latest rows, {Errors} errors", 
                updated, deletedProductDetailIds.Count, errors);

            return deletedProductDetailIds;
        }

        private void ProcessVisitorPeople(List<VisitorPersonModel>? visitorPeople)
        {
            if (visitorPeople == null || visitorPeople.Count == 0)
            {
                _logger.LogInformation("No VisitorPeople relations received from Mahak");
                return;
            }

            foreach (var relation in visitorPeople)
            {
                _logger.LogInformation(
                    "Mahak VisitorPerson relation {Status}: VisitorPersonId={VisitorPersonId}, VisitorId={VisitorId}, PersonId={PersonId}, RowVersion={RowVersion}",
                    relation.Deleted ? "DELETED" : "ACTIVE",
                    relation.VisitorPersonId,
                    relation.VisitorId,
                    relation.PersonId,
                    relation.RowVersion);
            }
        }

        private async Task ProcessInventoryAsync(
            List<ProductDetailStoreAssetModel>? inventory,
            HashSet<int> deletedProductDetailIds,
            CancellationToken cancellationToken)
        {
            if (inventory == null || !inventory.Any())
            {
                _logger.LogInformation("No inventory to process from Mahak");
                return;
            }
            
            _logger.LogInformation("Processing {Count} inventory records from Mahak", inventory.Count);
            
            int updated = 0;
            int errors = 0;

            var defaultStoreId = int.TryParse(_configuration["Mahak:DefaultStoreId"], out var configuredStoreId)
                ? configuredStoreId
                : 31940;

            // GetAllData is incremental and can contain multiple stores for one detail. Collapse only
            // the configured sales store before updating local variants, otherwise a zero record from
            // another store can overwrite the website stock.
            var inventoryByProductDetail = inventory
                .Where(i => !i.Deleted &&
                            i.StoreId == defaultStoreId &&
                            !deletedProductDetailIds.Contains(i.ProductDetailId))
                .GroupBy(i => i.ProductDetailId)
                .Select(group => new
                {
                    ProductDetailId = group.Key,
                    ProductDetailStoreAssetId = group
                        .OrderByDescending(i => i.RowVersion)
                        .First()
                        .ProductDetailStoreAssetId,
                    Quantity = NormalizeMahakStockQuantity(
                        group.Key,
                        group.Sum(i => i.Count1)),
                    UpdateDate = group.Max(i => i.UpdateDate ?? i.CreateDate)
                })
                .ToList();

            if (deletedProductDetailIds.Count > 0)
            {
                _logger.LogInformation(
                    "Skipped inventory for {Count} ProductDetailIds because their latest ProductDetail row is deleted",
                    deletedProductDetailIds.Count);
            }

            if (inventoryByProductDetail.Count == 0)
            {
                _logger.LogInformation("No inventory records found for configured Mahak store {StoreId}", defaultStoreId);
                return;
            }

            // Map ProductDetailIds to ProductIds and group
            var affectedProductIds = new HashSet<Guid>();
            var directProductQuantities = new Dictionary<Guid, int>();
            var directProductUpdateDates = new Dictionary<Guid, DateTime>();
            var productUpdateDates = new Dictionary<Guid, DateTime>();
            
            foreach (var inv in inventoryByProductDetail)
            {
                if (!inv.UpdateDate.HasValue)
                {
                    _logger.LogWarning(
                        "Skipping Mahak inventory for ProductDetailId {ProductDetailId}: UpdateDate is missing",
                        inv.ProductDetailId);
                    continue;
                }

                var mahakUpdatedAt = NormalizeMahakDate(inv.UpdateDate.Value);
                var productDetail = await _productDetailRepository.GetByMahakIdIgnoreFiltersAsync(
                    inv.ProductDetailId,
                    cancellationToken);

                if (productDetail?.Deleted == true)
                {
                    _logger.LogInformation(
                        "Skipping Mahak inventory for ProductDetailId {ProductDetailId}: local ProductDetail is deleted",
                        inv.ProductDetailId);
                    continue;
                }

                Guid? productId = productDetail?.ProductId;
                if (!productId.HasValue)
                {
                    var productDetailMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "ProductDetail", inv.ProductDetailId, cancellationToken);
                    productId = productDetailMapping?.LocalEntityId;
                }

                if (productId.HasValue)
                {
                    var localProductId = productId.Value;
                    affectedProductIds.Add(localProductId);

                    var variant = await _productVariantRepository.GetByMahakIdAsync(
                        inv.ProductDetailStoreAssetId,
                        cancellationToken);

                    if (variant == null)
                    {
                        var variantMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                            "ProductVariant", inv.ProductDetailId, cancellationToken);
                        if (variantMapping != null)
                        {
                            variant = await _productVariantRepository.GetByIdAsync(
                                variantMapping.LocalEntityId,
                                cancellationToken);
                        }
                    }

                    if (variant != null)
                    {
                        if (ShouldApplyMahakInventory(
                                variant.UpdatedAt ?? variant.CreatedAt,
                                mahakUpdatedAt,
                                variant.StockQuantity,
                                inv.Quantity))
                        {
                            variant.SetMahakVariantId(inv.ProductDetailStoreAssetId);
                            variant.SetStockQuantity(inv.Quantity, mahakUpdatedAt);
                            await _productVariantRepository.UpdateAsync(variant, cancellationToken);
                            SetMaxUpdateDate(productUpdateDates, localProductId, mahakUpdatedAt);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "Skipped Mahak inventory for variant {VariantId}: Mahak UpdateDate {MahakUpdatedAt} is not newer than local UpdatedAt {LocalUpdatedAt}, or quantity is unchanged",
                                variant.Id,
                                mahakUpdatedAt,
                                variant.UpdatedAt ?? variant.CreatedAt);
                        }
                    }
                    else
                    {
                        // Fallback: variants created from ProductDetail use "PD-{ProductDetailId}" SKU.
                        var fallbackSku = $"PD-{inv.ProductDetailId}";
                        var variantBySku = await _productVariantRepository.GetBySKUAsync(fallbackSku, cancellationToken);
                        if (variantBySku != null)
                        {
                            if (ShouldApplyMahakInventory(
                                    variantBySku.UpdatedAt ?? variantBySku.CreatedAt,
                                    mahakUpdatedAt,
                                    variantBySku.StockQuantity,
                                    inv.Quantity))
                            {
                                variantBySku.SetMahakVariantId(inv.ProductDetailStoreAssetId);
                                variantBySku.SetStockQuantity(inv.Quantity, mahakUpdatedAt);
                                await _productVariantRepository.UpdateAsync(variantBySku, cancellationToken);

                                var existingVariantMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                                    "ProductVariant",
                                    inv.ProductDetailId,
                                    cancellationToken);
                                if (existingVariantMapping == null)
                                {
                                    var newVariantMapping = MahakMapping.Create(
                                        entityType: "ProductVariant",
                                        localEntityId: variantBySku.Id,
                                        mahakEntityId: inv.ProductDetailId);
                                    await _mahakMappingRepository.AddAsync(newVariantMapping, cancellationToken);
                                }

                                SetMaxUpdateDate(productUpdateDates, localProductId, mahakUpdatedAt);
                            }
                            else
                            {
                                _logger.LogInformation(
                                    "Skipped Mahak inventory for variant SKU {Sku}: Mahak UpdateDate {MahakUpdatedAt} is not newer than local UpdatedAt {LocalUpdatedAt}, or quantity is unchanged",
                                    fallbackSku,
                                    mahakUpdatedAt,
                                    variantBySku.UpdatedAt ?? variantBySku.CreatedAt);
                            }
                        }
                        else
                        {
                            directProductQuantities[localProductId] = directProductQuantities.GetValueOrDefault(localProductId) + inv.Quantity;
                            SetMaxUpdateDate(directProductUpdateDates, localProductId, mahakUpdatedAt);
                        }
                    }
                }
            }

            // Recalculate totals from all locally stored variants. The Mahak response only contains
            // changed rows, so summing the response itself would zero or undercount unaffected variants.
            foreach (var productId in affectedProductIds)
            {
                try
                {
                    var product = await _productRepository.GetByIdTrackedAsync(productId, cancellationToken);
                    
                    if (product == null)
                    {
                        _logger.LogWarning("Product entity not found for ProductId {ProductId}", productId);
                        continue;
                    }

                    var variants = await _productVariantRepository.GetByProductIdAsync(productId, cancellationToken);
                    var totalQuantity = variants.Count > 0
                        ? variants.Sum(v => v.StockQuantity)
                        : directProductQuantities.GetValueOrDefault(productId);

                    if (variants.Count > 0)
                    {
                        if (product.StockQuantity == totalQuantity)
                        {
                            continue;
                        }

                        var productStockUpdatedAt = productUpdateDates.GetValueOrDefault(productId);
                        if (productStockUpdatedAt == default)
                        {
                            productStockUpdatedAt = variants.Max(v => v.UpdatedAt ?? v.CreatedAt);
                        }

                        product.SetStockQuantity(totalQuantity, NormalizeLocalDate(productStockUpdatedAt));
                    }
                    else
                    {
                        var mahakUpdatedAt = directProductUpdateDates.GetValueOrDefault(productId);
                        if (mahakUpdatedAt == default)
                        {
                            continue;
                        }

                        if (!ShouldApplyMahakInventory(
                                product.UpdatedAt ?? product.CreatedAt,
                                mahakUpdatedAt,
                                product.StockQuantity,
                                totalQuantity))
                        {
                            _logger.LogInformation(
                                "Skipped Mahak inventory total for product {ProductId}: Mahak UpdateDate {MahakUpdatedAt} is not newer than local UpdatedAt {LocalUpdatedAt}, or quantity is unchanged",
                                product.Id,
                                mahakUpdatedAt,
                                product.UpdatedAt ?? product.CreatedAt);
                            continue;
                        }

                        product.SetStockQuantity(totalQuantity, mahakUpdatedAt);
                    }
                    
                    // Save changes
                    try
                    {
                        await _productRepository.UpdateAsync(product, cancellationToken);
                        updated++;
                        _logger.LogInformation("Updated inventory for product {ProductName}: {Quantity} (from all variants)", 
                            product.Name, totalQuantity);
                    }
                    catch (Exception updateEx)
                    {
                        _logger.LogWarning(updateEx, "Failed to update product {ProductId}", product.Id);
                        errors++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing inventory for ProductId {ProductId}: {Error}", 
                        productId, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "Inventory sync completed: {Updated} updated, {Errors} errors", 
                updated, errors);
        }

        private static bool ShouldApplyMahakInventory(
            DateTime localUpdatedAt,
            DateTime mahakUpdatedAt,
            int localQuantity,
            int mahakQuantity)
        {
            if (localQuantity == mahakQuantity)
            {
                return false;
            }

            var normalizedLocalUpdatedAt = NormalizeLocalDate(localUpdatedAt);
            if (mahakUpdatedAt > normalizedLocalUpdatedAt)
            {
                return true;
            }

            // ProductDetailStoreAsset is the inventory source of truth. RowVersion already decides
            // whether Mahak sent this inventory row in the sync window, so a stale or timezone-shifted
            // UpdateDate must not leave a variant stuck at an older quantity.
            return true;
        }

        private static DateTime NormalizeMahakDate(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static DateTime NormalizeLocalDate(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private int NormalizeMahakStockQuantity(int productDetailId, decimal quantity)
        {
            if (quantity < 0)
            {
                _logger.LogWarning(
                    "Mahak returned negative inventory {Quantity} for ProductDetailId {ProductDetailId}; clamping to 0",
                    quantity,
                    productDetailId);
                return 0;
            }

            if (quantity > int.MaxValue)
            {
                _logger.LogWarning(
                    "Mahak returned too large inventory {Quantity} for ProductDetailId {ProductDetailId}; clamping to {MaxQuantity}",
                    quantity,
                    productDetailId,
                    int.MaxValue);
                return int.MaxValue;
            }

            return (int)quantity;
        }

        private static void SetMaxUpdateDate(Dictionary<Guid, DateTime> updates, Guid productId, DateTime updateDate)
        {
            if (!updates.TryGetValue(productId, out var current) || updateDate > current)
            {
                updates[productId] = updateDate;
            }
        }

        private async Task ProcessImagesAsync(List<PhotoGalleryModel>? galleries, List<PictureModel>? pictures, CancellationToken cancellationToken)
        {
            var latestPictures = (pictures ?? new List<PictureModel>())
                .GroupBy(picture => picture.PictureId)
                .Select(group => group.OrderByDescending(picture => picture.RowVersion).First())
                .ToList();
            var latestGalleries = (galleries ?? new List<PhotoGalleryModel>())
                .Where(gallery => gallery.EntityType == 102)
                .GroupBy(gallery => gallery.PhotoGalleryId)
                .Select(group => group.OrderByDescending(gallery => gallery.RowVersion).First())
                .ToList();

            if (latestPictures.Count == 0 && latestGalleries.Count == 0)
            {
                return;
            }

            foreach (var picture in latestPictures)
            {
                var snapshot = new MahakPictureSnapshot(
                    picture.PictureId,
                    picture.PictureClientId,
                    picture.Url,
                    picture.Deleted,
                    picture.RowVersion);
                await UpsertMediaSnapshotAsync(
                    MahakPictureMappingType,
                    picture.PictureId,
                    picture.RowVersion,
                    JsonSerializer.Serialize(snapshot),
                    !picture.Deleted && !string.IsNullOrWhiteSpace(picture.Url),
                    cancellationToken);
            }

            foreach (var gallery in latestGalleries)
            {
                var snapshot = new MahakGallerySnapshot(
                    gallery.PhotoGalleryId,
                    gallery.PhotoGalleryClientId,
                    gallery.ItemCode,
                    gallery.PictureId,
                    gallery.IsMain,
                    gallery.Deleted,
                    gallery.RowVersion);
                await UpsertMediaSnapshotAsync(
                    MahakPhotoGalleryMappingType,
                    gallery.PhotoGalleryId,
                    gallery.RowVersion,
                    JsonSerializer.Serialize(snapshot),
                    !gallery.Deleted,
                    cancellationToken);
            }

            var allMappings = (await _mahakMappingRepository.GetAllAsync(cancellationToken)).ToList();
            var pictureSnapshots = allMappings
                .Where(mapping => mapping.EntityType == MahakPictureMappingType)
                .Select(mapping => new
                {
                    Mapping = mapping,
                    Snapshot = DeserializeSnapshot<MahakPictureSnapshot>(mapping.Notes)
                })
                .Where(entry => entry.Snapshot != null)
                .ToDictionary(entry => entry.Snapshot!.PictureId, entry => entry);
            var gallerySnapshots = allMappings
                .Where(mapping => mapping.EntityType == MahakPhotoGalleryMappingType)
                .Select(mapping => new
                {
                    Mapping = mapping,
                    Snapshot = DeserializeSnapshot<MahakGallerySnapshot>(mapping.Notes)
                })
                .Where(entry => entry.Snapshot != null)
                .ToList();

            var changedPictureIds = latestPictures.Select(picture => picture.PictureId).ToHashSet();
            var affectedItemCodes = latestGalleries.Select(gallery => gallery.ItemCode).ToHashSet();
            foreach (var entry in gallerySnapshots.Where(entry =>
                         entry.Mapping.MappingStatus == "Active" &&
                         changedPictureIds.Contains(entry.Snapshot!.PictureId)))
            {
                affectedItemCodes.Add(entry.Snapshot!.ItemCode);
            }

            int newImages = 0;
            int deletedImages = 0;
            int errors = 0;

            foreach (var itemCode in affectedItemCodes)
            {
                try
                {
                    var productMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "Product", itemCode, cancellationToken);
                    if (productMapping == null)
                    {
                        _logger.LogWarning("Product with MahakId {ItemCode} not found in mappings", itemCode);
                        continue;
                    }

                    var product = await _productRepository.GetByIdIgnoreFiltersAsync(
                        productMapping.LocalEntityId, cancellationToken);
                    if (product == null || product.MahakId != itemCode || product.Deleted || product.DeletedByMahak)
                    {
                        _logger.LogInformation(
                            "Skipping gallery reconciliation for inactive or stale product mapping: MahakId={ItemCode}, LocalId={LocalId}",
                            itemCode,
                            productMapping.LocalEntityId);
                        continue;
                    }

                    var desiredImages = gallerySnapshots
                        .Where(entry =>
                            entry.Mapping.MappingStatus == "Active" &&
                            entry.Snapshot!.ItemCode == itemCode)
                        .Select(entry => entry.Snapshot!)
                        .Where(gallery =>
                            pictureSnapshots.TryGetValue(gallery.PictureId, out var pictureEntry) &&
                            pictureEntry.Mapping.MappingStatus == "Active" &&
                            !string.IsNullOrWhiteSpace(pictureEntry.Snapshot!.Url))
                        .Select(gallery => new
                        {
                            Gallery = gallery,
                            Picture = pictureSnapshots[gallery.PictureId].Snapshot!
                        })
                        .GroupBy(entry => entry.Picture.PictureId)
                        .Select(group => group.OrderByDescending(entry => entry.Gallery.RowVersion).First())
                        .OrderByDescending(entry => entry.Gallery.IsMain)
                        .ThenBy(entry => entry.Gallery.PhotoGalleryId)
                        .ToList();

                    var existingImages = (await _productImageRepository.GetByProductIdAsync(
                        product.Id, cancellationToken)).ToList();
                    var desiredPictureIds = desiredImages.Select(entry => entry.Picture.PictureId).ToHashSet();

                    foreach (var staleImage in existingImages.Where(image =>
                                 TryGetMahakPictureId(image, out var pictureId) &&
                                 !desiredPictureIds.Contains(pictureId)).ToList())
                    {
                        await _productImageRepository.DeleteAsync(staleImage.Id, cancellationToken);
                        existingImages.Remove(staleImage);
                        deletedImages++;
                    }

                    for (var index = 0; index < desiredImages.Count; index++)
                    {
                        var desired = desiredImages[index];
                        var isPrimary = index == 0;
                        var existing = existingImages.FirstOrDefault(image =>
                            image.MahakId == desired.Picture.PictureId ||
                            string.Equals(image.ImageUrl, desired.Picture.Url, StringComparison.OrdinalIgnoreCase));

                        if (existing == null)
                        {
                            existing = ProductImage.Create(
                                product.Id,
                                desired.Picture.Url!,
                                product.Name,
                                product.Name,
                                index,
                                isPrimary,
                                isPrimary ? "Main" : "Gallery");
                            existing.ApplyMahakIdentity(
                                desired.Picture.PictureId,
                                desired.Gallery.PhotoGalleryId,
                                desired.Picture.PictureClientId);
                            await _productImageRepository.AddAsync(existing, cancellationToken);
                            existingImages.Add(existing);
                            newImages++;
                            continue;
                        }

                        existing.Update(
                            desired.Picture.Url!,
                            product.Name,
                            product.Name,
                            index,
                            isPrimary,
                            isPrimary ? "Main" : "Gallery",
                            existing.FileSize,
                            existing.MimeType,
                            null);
                        existing.ApplyMahakIdentity(
                            desired.Picture.PictureId,
                            desired.Gallery.PhotoGalleryId,
                            desired.Picture.PictureClientId);
                        await _productImageRepository.UpdateAsync(existing, cancellationToken);
                    }

                    var desiredPrimary = existingImages.FirstOrDefault(image =>
                        desiredImages.Count > 0 &&
                        (image.MahakId == desiredImages[0].Picture.PictureId ||
                         string.Equals(image.ImageUrl, desiredImages[0].Picture.Url, StringComparison.OrdinalIgnoreCase)));
                    if (desiredPrimary != null)
                    {
                        await _productImageRepository.SetPrimaryImageAsync(
                            product.Id, desiredPrimary.Id, cancellationToken);
                    }

                    _logger.LogInformation(
                        "Reconciled Mahak gallery for {ProductName} (MahakId: {ItemCode}): {Count} active images",
                        product.Name,
                        itemCode,
                        desiredImages.Count);
                }
                catch (Exception ex)
                {
                    errors++;
                    _logger.LogError(ex, "Error reconciling images for product {ItemCode}", itemCode);
                }
            }
            
            _logger.LogInformation("Image sync completed: {NewImages} new, {DeletedImages} deleted, {Errors} errors", newImages, deletedImages, errors);
        }

        private async Task UpsertMediaSnapshotAsync(
            string entityType,
            int mahakEntityId,
            long mahakRowVersion,
            string snapshot,
            bool active,
            CancellationToken cancellationToken)
        {
            var mapping = await _mahakMappingRepository.GetByMahakEntityIdIgnoreStatusAsync(
                entityType, mahakEntityId, cancellationToken);
            if (mapping != null &&
                long.TryParse(mapping.MahakEntityCode, out var storedRowVersion) &&
                storedRowVersion > mahakRowVersion)
            {
                return;
            }

            if (mapping == null)
            {
                mapping = MahakMapping.Create(
                    entityType,
                    Guid.NewGuid(),
                    mahakEntityId,
                    mahakRowVersion.ToString(),
                    snapshot);
                if (!active)
                {
                    mapping.Unmap("Deleted by Mahak", snapshot);
                }

                await _mahakMappingRepository.AddAsync(mapping, cancellationToken);
                return;
            }

            mapping.Update(mahakEntityId, mahakRowVersion.ToString(), snapshot, "MahakSync");
            if (active)
            {
                mapping.Reactivate(snapshot);
            }
            else
            {
                mapping.Unmap("Deleted by Mahak", snapshot);
            }

            await _mahakMappingRepository.UpdateAsync(mapping, cancellationToken);
        }

        private static string BuildProductSyncNotes(long rowVersion, string action)
            => $"ProductRowVersion={rowVersion}; {action} on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

        private static bool TryGetStoredProductRowVersion(string? notes, out long rowVersion)
        {
            const string marker = "ProductRowVersion=";
            if (!string.IsNullOrWhiteSpace(notes))
            {
                var markerIndex = notes.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex >= 0)
                {
                    var valueStart = markerIndex + marker.Length;
                    var valueEnd = notes.IndexOf(';', valueStart);
                    var value = valueEnd >= 0
                        ? notes[valueStart..valueEnd]
                        : notes[valueStart..];
                    if (long.TryParse(value, out rowVersion))
                    {
                        return true;
                    }
                }
            }

            rowVersion = 0;
            return false;
        }

        private static T? DeserializeSnapshot<T>(string? json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static bool TryGetMahakPictureId(ProductImage image, out int pictureId)
        {
            if (image.MahakId.HasValue)
            {
                pictureId = image.MahakId.Value;
                return true;
            }

            var fileName = Path.GetFileName(image.ImageUrl);
            const string prefix = "pic-";
            if (fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var separatorIndex = fileName.IndexOf('-', prefix.Length);
                if (separatorIndex > prefix.Length &&
                    int.TryParse(fileName[prefix.Length..separatorIndex], out pictureId))
                {
                    return true;
                }
            }

            pictureId = 0;
            return false;
        }

        private sealed record MahakPictureSnapshot(
            int PictureId,
            int PictureClientId,
            string? Url,
            bool Deleted,
            long RowVersion);

        private sealed record MahakGallerySnapshot(
            int PhotoGalleryId,
            int PhotoGalleryClientId,
            int ItemCode,
            int PictureId,
            bool IsMain,
            bool Deleted,
            long RowVersion);

        private async Task ProcessPeopleAsync(List<PersonModel>? people, CancellationToken cancellationToken)
        {
            if (people == null || !people.Any())
            {
                _logger.LogInformation("No people to process from Mahak");
                return;
            }

            _logger.LogInformation("Processing {Count} people from Mahak", people.Count);

            int created = 0;
            int updated = 0;
            int errors = 0;

            foreach (var mahakPerson in people)
            {
                try
                {
                    if (mahakPerson.Deleted)
                    {
                        _logger.LogDebug("Skipping deleted person: {PersonId}", mahakPerson.PersonId);
                        continue;
                    }

                    // Check if person already exists in mapping
                    var mapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "Person",
                        mahakPerson.PersonId,
                        cancellationToken);

                    if (mapping != null)
                    {
                        var mappedUser = await _userManager.FindByIdAsync(mapping.LocalEntityId.ToString());
                        if (mappedUser != null)
                        {
                            var needsUpdate = mappedUser.MahakPersonId != mahakPerson.PersonId ||
                                              mappedUser.MahakPersonClientId != mahakPerson.PersonClientId;

                            if (needsUpdate)
                            {
                                mappedUser.MahakPersonId = mahakPerson.PersonId;
                                mappedUser.MahakPersonClientId = mahakPerson.PersonClientId;
                                mappedUser.MahakSyncedAt = DateTime.UtcNow;
                                await _userManager.UpdateAsync(mappedUser);
                            }
                        }

                        _logger.LogDebug("Person {PersonId} ({Name} {Family}) already mapped to local entity {LocalId}",
                            mahakPerson.PersonId, mahakPerson.FirstName, mahakPerson.LastName, mapping.LocalEntityId);
                        updated++;
                    }
                    else
                    {
                        var matchedUser = _userManager.Users.FirstOrDefault(u =>
                            (u.MahakPersonClientId.HasValue && u.MahakPersonClientId.Value == mahakPerson.PersonClientId) ||
                            (!string.IsNullOrWhiteSpace(mahakPerson.Mobile) && u.PhoneNumber == mahakPerson.Mobile) ||
                            (!string.IsNullOrWhiteSpace(mahakPerson.Email) && u.Email == mahakPerson.Email));

                        if (matchedUser != null)
                        {
                            matchedUser.MahakPersonId = mahakPerson.PersonId;
                            matchedUser.MahakPersonClientId = mahakPerson.PersonClientId;
                            matchedUser.MahakSyncedAt = DateTime.UtcNow;
                            await _userManager.UpdateAsync(matchedUser);

                            var userMapping = MahakMapping.Create(
                                "Person",
                                matchedUser.Id,
                                mahakPerson.PersonId,
                                mahakPerson.PersonCode.ToString(),
                                $"Matched incoming Mahak person on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                            await _mahakMappingRepository.AddAsync(userMapping, cancellationToken);
                            updated++;
                        }
                        else
                        {
                            // New person from Mahak - create mapping with placeholder GUID
                            var placeholderGuid = Guid.NewGuid();
                            var newMapping = MahakMapping.Create(
                                "Person",
                                placeholderGuid,
                                mahakPerson.PersonId,
                                mahakPerson.PersonCode.ToString());

                            await _mahakMappingRepository.AddAsync(newMapping, cancellationToken);

                            _logger.LogInformation("New person from Mahak: {PersonId} - {Name} {Family} (Mobile: {Mobile})",
                                mahakPerson.PersonId, mahakPerson.FirstName, mahakPerson.LastName, mahakPerson.Mobile);
                            created++;
                        }
                    }

                    // Save RowVersion for this person
                    await LogSyncAsync("Person", mahakPerson.RowVersion, 1, "Success", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing person {PersonId}: {Error}", 
                        mahakPerson.PersonId, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "People sync completed: {Created} new, {Updated} existing, {Errors} errors",
                created, updated, errors);
        }

        private async Task UpsertProductDetailFromMahakDetail(ProductDetailModel detail, Product product, CancellationToken cancellationToken)
        {
            var key = $"MahakProductDetail:{detail.ProductDetailId}";
            var value = TruncateForProductDetailValue(
                !string.IsNullOrWhiteSpace(detail.Properties)
                    ? detail.Properties
                    : detail.Barcode ?? detail.ProductDetailCode.ToString());

            var existingDetail = await _productDetailRepository.GetByMahakIdIgnoreFiltersAsync(
                detail.ProductDetailId,
                cancellationToken);
            var isNew = existingDetail == null;

            if (existingDetail == null)
            {
                existingDetail = ProductDetail.Create(
                    product.Id,
                    key,
                    value,
                    detail.Barcode,
                    0);
            }
            else
            {
                if (existingDetail.ProductId != product.Id)
                {
                    _logger.LogWarning(
                        "Reassigning reused Mahak ProductDetailId {DetailId} from product {OldProductId} to {NewProductId}",
                        detail.ProductDetailId,
                        existingDetail.ProductId,
                        product.Id);
                    existingDetail.ReassignToProduct(product.Id);
                }

                existingDetail.Update(key, value, detail.Barcode, existingDetail.DisplayOrder, null);
            }

            existingDetail.ApplyMahakDetail(
                detail.ProductDetailId,
                detail.ProductDetailClientId,
                value,
                detail.Deleted);

            if (isNew)
            {
                await _productDetailRepository.AddAsync(existingDetail, cancellationToken);
            }
            else
            {
                await _productDetailRepository.UpdateAsync(existingDetail, cancellationToken);
            }
        }

        private async Task EnsureProductDetailMapping(ProductDetailModel detail, Product product, CancellationToken cancellationToken)
        {
            var existingDetailMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                "ProductDetail", detail.ProductDetailId, cancellationToken);
            
            if (existingDetailMapping == null)
            {
                var detailMapping = MahakMapping.Create(
                    entityType: "ProductDetail",
                    localEntityId: product.Id,
                    mahakEntityId: detail.ProductDetailId
                );
                await _mahakMappingRepository.AddAsync(detailMapping, cancellationToken);
                _logger.LogDebug("Created ProductDetail mapping: {DetailId} -> Product {ProductId}", 
                    detail.ProductDetailId, product.Id);
            }
            else if (existingDetailMapping.LocalEntityId != product.Id)
            {
                existingDetailMapping.SetLocalEntityId(product.Id);
                existingDetailMapping.Reactivate("Reassigned reused ProductDetailId to its current Mahak product");
                await _mahakMappingRepository.UpdateAsync(existingDetailMapping, cancellationToken);
            }
        }

        private async Task UpsertVariantFromDetail(ProductDetailModel detail, Product product, CancellationToken cancellationToken)
        {
            var parsed = ParseProperties(detail.Properties);
            var color = parsed.color;
            var size = parsed.size;
            if (string.IsNullOrWhiteSpace(color) && string.IsNullOrWhiteSpace(size))
            {
                return; // nothing to map
            }

            var variantMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                "ProductVariant", detail.ProductDetailId, cancellationToken);

            var sku = !string.IsNullOrWhiteSpace(detail.Barcode)
                ? detail.Barcode
                : $"PD-{detail.ProductDetailId}";

            if (variantMapping != null)
            {
                var existing = await _productVariantRepository.GetByIdAsync(variantMapping.LocalEntityId, cancellationToken);
                if (existing != null && existing.ProductId == product.Id)
                {
                    // ProductDetail.Count1 is not store inventory. Preserve stock until
                    // ProductDetailStoreAssets supplies the configured store quantity.
                    existing.UpdateMahakDetailMetadata(
                        detail.ProductDetailClientId,
                        size ?? existing.Size,
                        color ?? existing.Color,
                        sku,
                        existing.AdditionalPrice,
                        detail.Barcode,
                        parsed.feature8Value,
                        parsed.feature9Value);
                    await _productVariantRepository.UpdateAsync(existing, cancellationToken);
                    return;
                }

                if (existing != null)
                {
                    _logger.LogWarning(
                        "Removing stale variant mapping for reused ProductDetailId {DetailId}: old product {OldProductId}, new product {NewProductId}",
                        detail.ProductDetailId,
                        existing.ProductId,
                        product.Id);
                    await _productVariantRepository.DeleteAsync(existing.Id, cancellationToken);
                }
            }

            // Create new variant
            var variant = ProductVariant.Create(product.Id,
                size ?? "یک‌سایز",
                color ?? "نامشخص",
                sku,
                0);

            variant.UpdateMahakDetailMetadata(
                detail.ProductDetailClientId,
                size ?? "یک‌سایز",
                color ?? "نامشخص",
                sku,
                variant.AdditionalPrice,
                detail.Barcode,
                parsed.feature8Value,
                parsed.feature9Value);
            // Additional price not provided from Mahak detail; skip.

            await _productVariantRepository.AddAsync(variant, cancellationToken);

            if (variantMapping == null)
            {
                var newVariantMapping = MahakMapping.Create(
                    entityType: "ProductVariant",
                    localEntityId: variant.Id,
                    mahakEntityId: detail.ProductDetailId
                );
                await _mahakMappingRepository.AddAsync(newVariantMapping, cancellationToken);
            }
            else
            {
                variantMapping.SetLocalEntityId(variant.Id);
                variantMapping.Reactivate("Reassigned reused ProductDetailId to its current variant");
                await _mahakMappingRepository.UpdateAsync(variantMapping, cancellationToken);
            }
            _logger.LogDebug("Created ProductVariant from detail {DetailId}: Color={Color}, Size={Size}", detail.ProductDetailId, color, size);
        }

        private static string TruncateForProductDetailValue(string value)
        {
            value = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
            return value.Length <= 500 ? value : value[..500];
        }

        private async Task DeleteVariantFromDetail(ProductDetailModel detail, CancellationToken cancellationToken)
        {
            try
            {
                var variantMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                    "ProductVariant",
                    detail.ProductDetailId,
                    cancellationToken);

                if (variantMapping != null)
                {
                    await _productVariantRepository.DeleteAsync(variantMapping.LocalEntityId, cancellationToken);
                    await _mahakMappingRepository.DeleteAsync(variantMapping.Id, cancellationToken);
                    _logger.LogDebug("Deleted ProductVariant by mapping for ProductDetailId {DetailId}", detail.ProductDetailId);
                }

                var fallbackSku = $"PD-{detail.ProductDetailId}";
                var variantBySku = await _productVariantRepository.GetBySKUAsync(fallbackSku, cancellationToken);
                if (variantBySku != null)
                {
                    await _productVariantRepository.DeleteAsync(variantBySku.Id, cancellationToken);
                    _logger.LogDebug("Deleted ProductVariant by fallback SKU for ProductDetailId {DetailId}", detail.ProductDetailId);
                }

                var detailMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                    "ProductDetail",
                    detail.ProductDetailId,
                    cancellationToken);

                if (detailMapping != null)
                {
                    await _mahakMappingRepository.DeleteAsync(detailMapping.Id, cancellationToken);
                    _logger.LogDebug("Deleted ProductDetail mapping for deleted ProductDetailId {DetailId}", detail.ProductDetailId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete ProductVariant for ProductDetailId {DetailId}", detail.ProductDetailId);
            }
        }

        private (string? color, string? size, string? feature8Value, string? feature9Value) ParseProperties(string? propertiesJson)
        {
            if (string.IsNullOrWhiteSpace(propertiesJson))
                return (null, null, null, null);

            try
            {
                using var doc = JsonDocument.Parse(propertiesJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    return (null, null, null, null);

                string? color = null;
                string? size = null;
                string? feature8Value = null;
                string? feature9Value = null;

                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (!item.TryGetProperty("C", out var cProp) || !item.TryGetProperty("V", out var vProp))
                        continue;

                    var code = cProp.GetString();
                    var value = vProp.GetString();
                    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(value))
                        continue;

                    switch (code.Trim())
                    {
                        case "3": // color
                            color ??= value.Trim();
                            break;
                        case "4": // size
                            size ??= value.Trim();
                            break;
                        case "8":
                            feature8Value ??= value.Trim();
                            break;
                        case "9":
                            feature9Value ??= value.Trim();
                            break;
                    }
                }

                return (color, size, feature8Value, feature9Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse ProductDetail Properties JSON: {Json}", propertiesJson);
                return (null, null, null, null);
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
