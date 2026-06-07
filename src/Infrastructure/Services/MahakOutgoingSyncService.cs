using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Infrastructure.Mahak.Models;

namespace OnlineShop.Infrastructure.Services
{
    public class MahakOutgoingSyncService : IMahakCustomerSyncService, IMahakOrderSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MahakOutgoingSyncService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserOrderRepository _orderRepository;
        private readonly IMahakMappingRepository _mahakMappingRepository;
        private readonly IMahakSyncLogRepository _mahakSyncLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        private string? _token;
        private int _visitorId;
        private static readonly SemaphoreSlim TokenSemaphore = new(1, 1);
        private static string? _sharedToken;
        private static int _sharedVisitorId;
        private static DateTimeOffset _sharedTokenExpiresAt = DateTimeOffset.MinValue;
        private const string BaseUrl = "https://mahakacc.mahaksoft.com/API/v3/Sync/";

        public MahakOutgoingSyncService(
            HttpClient httpClient,
            ILogger<MahakOutgoingSyncService> logger,
            IConfiguration configuration,
            IUserOrderRepository orderRepository,
            IMahakMappingRepository mahakMappingRepository,
            IMahakSyncLogRepository mahakSyncLogRepository,
            UserManager<ApplicationUser> userManager)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _orderRepository = orderRepository;
            _mahakMappingRepository = mahakMappingRepository;
            _mahakSyncLogRepository = mahakSyncLogRepository;
            _userManager = userManager;
            
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (HasValidSharedToken())
            {
                ApplyAuthorizationHeader(_sharedToken!, _sharedVisitorId);
            }
        }

        /// <summary>
        /// Check if Mahak credentials are configured. If not, sync should be silently skipped.
        /// </summary>
        public bool IsConfigured()
        {
            var username = _configuration["Mahak:Username"];
            var password = _configuration["Mahak:Password"];
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }

        public async Task SyncOrdersToMahakAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Skip silently if Mahak is not configured
                if (!IsConfigured())
                {
                    _logger.LogDebug("Mahak outgoing sync skipped: Username/Password not configured in appsettings.json");
                    return;
                }

                _logger.LogInformation("Starting outgoing sync to Mahak...");

                // 1. Login
                await EnsureAuthenticatedAsync(cancellationToken);

                // 2. Get unsync orders (paid but not synced to Mahak)
                var unsyncedOrders = await GetUnsyncedOrdersAsync(cancellationToken);

                if (unsyncedOrders == null || !unsyncedOrders.Any())
                {
                    _logger.LogInformation("No orders to sync to Mahak");
                    return;
                }

                _logger.LogInformation("Found {Count} orders to sync to Mahak", unsyncedOrders.Count);

                // 3. Convert and send orders
                int success = 0;
                int failed = 0;

                foreach (var order in unsyncedOrders)
                {
                    try
                    {
                        await SendOrderToMahakAsync(order, cancellationToken);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync order {OrderId} to Mahak", order.Id);
                        failed++;
                    }
                }

                _logger.LogInformation("Outgoing sync completed: {Success} success, {Failed} failed", success, failed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during outgoing sync to Mahak");
            }
        }

        private async Task<List<UserOrder>> GetUnsyncedOrdersAsync(CancellationToken cancellationToken)
        {
            return await _orderRepository.GetUnsyncedOrdersAsync(cancellationToken);
        }

        public async Task SyncOrderToMahakAsync(Guid orderId, CancellationToken cancellationToken)
        {
            if (!IsConfigured())
            {
                _logger.LogDebug("Mahak direct order sync skipped: Username/Password not configured in appsettings.json");
                return;
            }

            await EnsureAuthenticatedAsync(cancellationToken);

            var order = await _orderRepository.GetByIdForMahakSyncAsync(orderId, cancellationToken);
            if (order == null)
            {
                throw new InvalidOperationException($"Order {orderId} not found for Mahak sync.");
            }

            if (order.SyncedToMahak)
            {
                _logger.LogInformation("Order {OrderId} already synced to Mahak. Skipping direct sync.", orderId);
                return;
            }

            await SendOrderToMahakAsync(order, cancellationToken);
        }

        private async Task SendOrderToMahakAsync(UserOrder order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending order {OrderId} to Mahak", order.Id);

            // Ensure customer is synced to Mahak first
            int mahakPersonId = 0;
            if (order.User != null)
            {
                try
                {
                    mahakPersonId = await EnsureCustomerSyncedAsync(order.User, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync customer for order {OrderId}, will send order without PersonId", order.Id);
                }
            }
            else
            {
                _logger.LogWarning("Order {OrderId} has no User loaded, cannot sync customer", order.Id);
            }

            // Convert order to Mahak format
            // ShippingAddress must be JSON format as per Mahak support
            string? shippingAddressJson = null;
            if (order.ShippingAddress != null)
            {
                shippingAddressJson = JsonSerializer.Serialize(new
                {
                    fullAddress = order.ShippingAddress.ToString(),
                    city = order.ShippingAddress.City,
                    state = order.ShippingAddress.State,
                    postalCode = order.ShippingAddress.PostalCode
                });
            }

            var mahakOrder = new MahakOrderModel
            {
                OrderClientId = order.Id.GetHashCode(), // Use hash of GUID as long
                VisitorId = _visitorId,
                PersonId = mahakPersonId > 0 ? mahakPersonId : null, // Link to customer if synced
                OrderType = 201, // Sales invoice
                OrderDate = order.CreatedAt,
                DeliveryDate = order.CreatedAt.AddDays(3), // Estimate
                Discount = order.DiscountAmount,
                DiscountType = 0, // Amount
                SendCost = order.ShippingAmount,
                OtherCost = 0,
                SettlementType = 1, // Cash (since payment is done)
                Immediate = false,
                Description = $"Website Order #{order.OrderNumber}",
                ShippingAddress = shippingAddressJson
            };

            var mahakOrderDetails = new List<MahakOrderDetailModel>();

            // Convert order items
            foreach (var item in order.OrderItems)
            {
                MahakMapping? productDetailMapping = null;

                if (item.VariantId.HasValue)
                {
                    productDetailMapping = await _mahakMappingRepository.GetByLocalEntityIdAsync(
                        "ProductVariant",
                        item.VariantId.Value,
                        cancellationToken);
                }

                productDetailMapping ??= await _mahakMappingRepository.GetByLocalEntityIdAsync(
                    "ProductDetail",
                    item.ProductId,
                    cancellationToken);

                if (productDetailMapping == null)
                {
                    throw new InvalidOperationException(
                        $"Mahak mapping not found for order item {item.Id} (ProductId: {item.ProductId}, VariantId: {item.VariantId}).");
                }

                // StoreId is required as per Mahak support
                // Get from configuration
                var storeIdStr = _configuration["Mahak:DefaultStoreId"];
                if (string.IsNullOrEmpty(storeIdStr) || !int.TryParse(storeIdStr, out int storeId))
                {
                    _logger.LogWarning("Mahak:DefaultStoreId not configured, using default value 31940");
                    storeId = 31940; // Default fallback
                }

                mahakOrderDetails.Add(new MahakOrderDetailModel
                {
                    OrderDetailClientId = item.Id.GetHashCode(),
                    ItemType = 1, // ProductDetail (required by Mahak v14+ when using ProductDetailId)
                    OrderClientId = mahakOrder.OrderClientId,
                    ProductDetailId = productDetailMapping.MahakEntityId,
                    StoreId = storeId, // REQUIRED by Mahak
                    Price = item.UnitPrice,
                    Count1 = item.Quantity,
                    Count2 = 0,
                    Discount = item.DiscountAmount ?? 0,
                    DiscountType = 0,
                    TaxPercent = 0,
                    ChargePercent = 0,
                    Description = item.ProductName,
                    Gift = 0
                });
            }

            if (mahakOrderDetails.Count == 0)
            {
                throw new InvalidOperationException($"Order {order.Id} has no Mahak order details. Sync aborted.");
            }

            // Send to Mahak
            var request = new SaveAllDataRequest
            {
                Orders = new List<MahakOrderModel> { mahakOrder },
                OrderDetails = mahakOrderDetails
            };

            // Log the request for debugging
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogDebug("Sending order to Mahak. Request: {Request}", requestJson);

            var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var response = await _httpClient.PostAsync("SaveAllDataV2", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send order to Mahak. Status: {response.StatusCode}, Content: {errorContent}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Order {OrderId} sent to Mahak successfully. Response: {Response}", 
                order.Id, responseText);

            // Mark order as synced
            order.SetMahakSynced(mahakOrder.OrderClientId.ToString());
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        public async Task SyncCustomerToMahakAsync(Guid userId, CancellationToken cancellationToken)
        {
            if (!IsConfigured())
            {
                _logger.LogDebug("Mahak direct customer sync skipped: Username/Password not configured in appsettings.json");
                return;
            }

            await EnsureAuthenticatedAsync(cancellationToken);

            var user = await _userManager.Users
                .Where(u => u.Id == userId)
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"User {userId} not found for Mahak sync.");
            }

            await EnsureCustomerSyncedAsync(user, cancellationToken);
        }

        private async Task<int> EnsureCustomerSyncedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            // If user already has Mahak Person ID, return it
            if (user.MahakPersonId.HasValue)
            {
                _logger.LogDebug("User {UserId} already synced to Mahak with PersonId {PersonId}", 
                    user.Id, user.MahakPersonId.Value);
                return user.MahakPersonId.Value;
            }

            if (user.MahakPersonClientId.HasValue)
            {
                _logger.LogDebug("User {UserId} already synced to Mahak with PersonClientId {PersonClientId}",
                    user.Id, user.MahakPersonClientId.Value);
                return Convert.ToInt32(user.MahakPersonClientId.Value);
            }

            _logger.LogInformation("Syncing customer {UserId} ({Name}) to Mahak", 
                user.Id, $"{user.FirstName} {user.LastName}");

            // Get PersonGroupId from configuration (required by Mahak)
            var personGroupIdStr = _configuration["Mahak:DefaultPersonGroupId"];
            if (string.IsNullOrEmpty(personGroupIdStr) || !int.TryParse(personGroupIdStr, out int personGroupId))
            {
                _logger.LogWarning("Mahak:DefaultPersonGroupId not configured, using default value 102479");
                personGroupId = 102479; // Default fallback
            }

            // Create Person model
            var personClientId = Math.Abs(user.Id.GetHashCode());
            var mahakPerson = new MahakPersonModel
            {
                PersonClientId = personClientId,
                PersonGroupId = personGroupId, // Required for creating person
                Name = user.FirstName,
                Family = user.LastName,
                Mobile = user.PhoneNumber ?? string.Empty,
                Email = user.Email,
                Type = 0, // Real person
                Deleted = false
            };

            // Prepare request with Person
            var request = new SaveAllDataRequest
            {
                People = new List<MahakPersonModel> { mahakPerson }
            };

            // Add profile picture if exists
            if (user.UserProfile?.ProfileImageUrl != null)
            {
                try
                {
                    var pictureModel = await CreateProfilePictureModelAsync(user, personClientId, cancellationToken);
                    if (pictureModel != null)
                    {
                        request.Pictures = new List<MahakPictureModel> { pictureModel };
                        _logger.LogInformation("Including profile picture for user {UserId}", user.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to prepare profile picture for user {UserId}, continuing without it", user.Id);
                }
            }

            // Create VisitorPeople link (REQUIRED for real sales with inventory deduction)
            var visitorPersonClientId = Math.Abs((user.Id.ToString() + "_visitor").GetHashCode());
            var visitorPerson = new MahakVisitorPersonModel
            {
                VisitorPersonClientId = visitorPersonClientId,
                VisitorId = _visitorId,
                PersonClientId = personClientId,
                Deleted = false
            };
            request.VisitorPeople = new List<MahakVisitorPersonModel> { visitorPerson };

            // Log the request for debugging
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogDebug("Sending customer to Mahak. Request: {Request}", requestJson);

            // Send to Mahak
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json-patch+json");

            var response = await _httpClient.PostAsync("SaveAllDataV2", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to sync customer to Mahak. Status: {Status}, Error: {Error}", 
                    response.StatusCode, error);
                throw new Exception($"Failed to sync customer to Mahak. Status: {response.StatusCode}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Customer {UserId} synced to Mahak successfully. PersonClientId: {PersonClientId}, VisitorPersonClientId: {VisitorPersonClientId}", 
                user.Id, personClientId, visitorPersonClientId);

            // Save PersonClientId to user
            // Note: Mahak doesn't return the server-generated PersonId in SaveAllDataV2 response
            // So we use personClientId as the identifier
            user.MahakPersonClientId = personClientId;
            user.MahakSyncedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            
            return personClientId;
        }

        private async Task<MahakPictureModel?> CreateProfilePictureModelAsync(ApplicationUser user, long personClientId, CancellationToken cancellationToken)
        {
            if (user.UserProfile?.ProfileImageUrl == null)
                return null;

            try
            {
                var imageUrl = user.UserProfile.ProfileImageUrl;
                
                // If it's a local file path, read it
                if (System.IO.File.Exists(imageUrl))
                {
                    var imageBytes = await System.IO.File.ReadAllBytesAsync(imageUrl, cancellationToken);
                    var base64 = Convert.ToBase64String(imageBytes);
                    var fileName = $"person-{personClientId}.jpg";

                    // Check size (should be < 300KB)
                    if (imageBytes.Length > 300 * 1024)
                    {
                        _logger.LogWarning("Profile image for user {UserId} is too large ({Size} bytes), skipping", 
                            user.Id, imageBytes.Length);
                        return null;
                    }

                    var pictureClientId = Math.Abs((user.Id.ToString() + "_picture").GetHashCode());
                    return new MahakPictureModel
                    {
                        PictureClientId = pictureClientId,
                        FileName = fileName,
                        BinaryData = base64, // Pure Base64 without prefix
                        Deleted = false
                    };
                }
                // If it's a URL, download it
                else if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                {
                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(uri, cancellationToken);
                    
                    // Check size (should be < 300KB)
                    if (imageBytes.Length > 300 * 1024)
                    {
                        _logger.LogWarning("Profile image for user {UserId} is too large ({Size} bytes), skipping", 
                            user.Id, imageBytes.Length);
                        return null;
                    }

                    var base64 = Convert.ToBase64String(imageBytes);
                    var fileName = $"person-{personClientId}.jpg";
                    var pictureClientId = Math.Abs((user.Id.ToString() + "_picture").GetHashCode());

                    return new MahakPictureModel
                    {
                        PictureClientId = pictureClientId,
                        FileName = fileName,
                        BinaryData = base64, // Pure Base64 without prefix
                        Deleted = false
                    };
                }
                else
                {
                    _logger.LogWarning("Profile image URL for user {UserId} is not valid: {Url}", user.Id, imageUrl);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating profile picture model for user {UserId}", user.Id);
                return null;
            }
        }

        private async Task LoginAsync(CancellationToken cancellationToken)
        {
            var username = _configuration["Mahak:Username"];
            var password = _configuration["Mahak:Password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Mahak configuration is incomplete");
            }

            // Hash password with MD5
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

            var content = new StringContent(JsonSerializer.Serialize(loginModel), System.Text.Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var response = await _httpClient.PostAsync("Login", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Mahak login failed. Status: {response.StatusCode}");
            }

            var result = await JsonSerializer.DeserializeAsync<MahakApiResult<LoginResultModel>>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);

            if (result == null || !result.Result || result.Data == null)
            {
                throw new Exception($"Mahak login failed. Message: {result?.Message}");
            }

            _token = result.Data.UserToken;
            _visitorId = (int)result.Data.VisitorId;
            _sharedToken = _token;
            _sharedVisitorId = _visitorId;
            _sharedTokenExpiresAt = ResolveSharedTokenExpiration();
            ApplyAuthorizationHeader(_token, _visitorId);

            _logger.LogInformation("Mahak outgoing sync login successful. VisitorId: {VisitorId}", _visitorId);
        }

        private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
        {
            if (HasValidSharedToken())
            {
                ApplyAuthorizationHeader(_sharedToken!, _sharedVisitorId);
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
                    ApplyAuthorizationHeader(_sharedToken!, _sharedVisitorId);
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

        private void ApplyAuthorizationHeader(string token, int visitorId)
        {
            _token = token;
            _visitorId = visitorId;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private DateTimeOffset ResolveSharedTokenExpiration()
        {
            var tokenCacheMinutes = _configuration.GetValue<int?>("Mahak:TokenCacheMinutes") ?? 20;
            tokenCacheMinutes = Math.Clamp(tokenCacheMinutes, 1, 120);
            return DateTimeOffset.UtcNow.AddMinutes(tokenCacheMinutes);
        }
    }
}
