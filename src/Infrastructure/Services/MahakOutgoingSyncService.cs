using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private readonly IMahakTrafficLogger _mahakTrafficLogger;
        private static readonly JsonSerializerOptions SaveAllDataJsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        private static readonly JsonSerializerOptions ShippingAddressJsonOptions = new(SaveAllDataJsonOptions)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

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
            UserManager<ApplicationUser> userManager,
            IMahakTrafficLogger mahakTrafficLogger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _orderRepository = orderRepository;
            _mahakMappingRepository = mahakMappingRepository;
            _mahakSyncLogRepository = mahakSyncLogRepository;
            _userManager = userManager;
            _mahakTrafficLogger = mahakTrafficLogger;
            
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

                // 2. Try syncing pending customers first, but do not block orders if customer sync fails.
                await SyncPendingCustomersToMahakAsync(cancellationToken);

                // 3. Get unsync orders (paid but not synced to Mahak)
                var unsyncedOrders = await GetUnsyncedOrdersAsync(cancellationToken);

                if (unsyncedOrders == null || !unsyncedOrders.Any())
                {
                    _logger.LogInformation("No orders to sync to Mahak");
                    return;
                }

                _logger.LogInformation("Found {Count} orders to sync to Mahak", unsyncedOrders.Count);

                // 4. Convert and send orders
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

        public async Task SyncPendingCustomersToMahakAsync(CancellationToken cancellationToken)
        {
            if (!IsConfigured())
            {
                return;
            }

            var pendingUsers = await _userManager.Users
                .Where(u => !u.MahakPersonClientId.HasValue)
                .Include(u => u.UserProfile)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);

            if (pendingUsers.Count == 0)
            {
                _logger.LogDebug("No pending customers to sync to Mahak");
                return;
            }

            _logger.LogInformation("Found {Count} pending customers to sync to Mahak", pendingUsers.Count);

            foreach (var user in pendingUsers)
            {
                try
                {
                    await EnsureCustomerSyncedAsync(user, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync pending customer {UserId} to Mahak", user.Id);
                }
            }
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

            if (order.User == null)
            {
                throw new InvalidOperationException($"User {order.UserId} was not loaded for order {order.Id}.");
            }

            var mahakPersonId = await EnsureCustomerSyncedAsync(order.User, cancellationToken);
            var orderClientId = order.Id.GetHashCode(); // Use hash of GUID as long
            var paidPayment = order.Payments
                .Where(p => string.Equals(p.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
                .FirstOrDefault();
            var productDiscountAmount = order.OrderItems.Sum(GetItemDiscountAmount);
            var mahakInvoiceDiscount = order.DiscountAmount + productDiscountAmount;

            _logger.LogInformation(
                "Mahak invoice totals for order {OrderId}: line discount {ProductDiscount}, coupon discount {OrderDiscount}, invoice discount {InvoiceDiscount}, payable amount {PayableAmount}, recorded payment {RecordedPayment}",
                order.Id,
                productDiscountAmount,
                order.DiscountAmount,
                mahakInvoiceDiscount,
                order.TotalAmount,
                paidPayment?.Amount);

            var mahakOrder = new MahakOrderModel
            {
                OrderClientId = orderClientId,
                VisitorId = _visitorId,
                PersonId = mahakPersonId,
                ReceiptClientId = paidPayment != null ? orderClientId : null,
                OrderType = 201, // Sales invoice
                OrderDate = order.CreatedAt,
                DeliveryDate = order.CreatedAt,
                Discount = mahakInvoiceDiscount,
                DiscountType = 0, // Amount
                SendCost = order.ShippingAmount,
                OtherCost = 0,
                SettlementType = 1, // Cash (since payment is done)
                Immediate = false,
                Description = BuildOrderDescription(order),
                ShippingAddress = BuildShippingAddressJson(order)
            };

            if (string.IsNullOrWhiteSpace(mahakOrder.ShippingAddress))
            {
                _logger.LogWarning(
                    "Order {OrderId} is missing shipping address data for Mahak sync. ShippingAddress will be omitted.",
                    order.Id);
            }

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

                var itemDiscount = GetItemDiscountAmount(item);
                var originalUnitPrice = GetOriginalUnitPrice(item, itemDiscount);

                mahakOrderDetails.Add(new MahakOrderDetailModel
                {
                    OrderDetailClientId = item.Id.GetHashCode(),
                    ItemType = 1, // ProductDetail (required by Mahak v14+ when using ProductDetailId)
                    OrderClientId = mahakOrder.OrderClientId,
                    ProductDetailId = productDetailMapping.MahakEntityId,
                    StoreId = storeId, // REQUIRED by Mahak
                    Price = originalUnitPrice,
                    Count1 = item.Quantity,
                    Count2 = 0,
                    Discount = itemDiscount,
                    DiscountType = 0,
                    TaxPercent = 0,
                    ChargePercent = 0,
                    Description = BuildOrderItemDescription(item),
                    Gift = 0
                });
            }

            if (mahakOrderDetails.Count == 0)
            {
                throw new InvalidOperationException($"Order {order.Id} has no Mahak order details. Sync aborted.");
            }

            List<MahakReceiptModel>? receipts = null;
            if (paidPayment != null)
            {
                var paidAt = paidPayment.PaidAt ?? order.UpdatedAt ?? DateTime.UtcNow;
                receipts = new List<MahakReceiptModel>
                {
                    new()
                    {
                        ReceiptClientId = orderClientId,
                        ReceiptCode = null,
                        PersonId = mahakPersonId,
                        VisitorId = _visitorId,
                        // Send the exact amount settled by the gateway, which already includes discounts.
                        CashAmount = paidPayment.Amount,
                        CashCode = null,
                        Description = $"Payment for order {order.OrderNumber}",
                        Date = paidAt,
                        ProjectId = null,
                        OrderId = null,
                        Deleted = false,
                        UpdateDate = paidAt,
                        OrderClientId = orderClientId,
                        OrderCode = null,
                        OrderType = mahakOrder.OrderType
                    }
                };
            }
            else
            {
                _logger.LogWarning(
                    "Order {OrderId} is being synced to Mahak without a paid payment record, so no receipt will be sent.",
                    order.Id);
            }

            // Send to Mahak
            var request = new SaveAllDataRequest
            {
                Orders = new List<MahakOrderModel> { mahakOrder },
                OrderDetails = mahakOrderDetails,
                Receipts = receipts
            };

            // Log the request for debugging
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions(SaveAllDataJsonOptions) { WriteIndented = true });
            _logger.LogDebug("Sending order to Mahak. Request: {Request}", requestJson);
            await _mahakTrafficLogger.LogRequestAsync(
                "SendSalesInvoice",
                "SaveAllDataV2",
                "این دیتا برای ارسال فاکتور فروش به محک است",
                request,
                cancellationToken);

            var content = new StringContent(JsonSerializer.Serialize(request, SaveAllDataJsonOptions), System.Text.Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var response = await _httpClient.PostAsync("SaveAllDataV2", content, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            await _mahakTrafficLogger.LogResponseAsync(
                "SendSalesInvoice",
                "SaveAllDataV2",
                "این دیتا پاسخ محک بعد از ارسال فاکتور فروش است",
                (int)response.StatusCode,
                response.IsSuccessStatusCode,
                responseText,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send order to Mahak. Status: {response.StatusCode}, Content: {responseText}");
            }

            var saveResult = DeserializeSaveResult(responseText, $"order {order.Id}", throwOnRejected: false);
            if (!saveResult.Result && IsAlreadyRegisteredOrderResult(saveResult))
            {
                _logger.LogWarning(
                    "Mahak reports order {OrderId} / {OrderNumber} is already registered. Marking local order as synced to prevent repeated sales invoice submissions.",
                    order.Id,
                    order.OrderNumber);

                order.SetMahakSynced($"client:{orderClientId}");
                await _orderRepository.UpdateAsync(order, cancellationToken);
                return;
            }

            EnsureSaveResultAccepted(saveResult, $"order {order.Id}", responseText);
            var orderResult = GetSuccessfulEntityResult(saveResult.Data?.Objects?.Orders, "Orders");
            GetSuccessfulEntityResult(saveResult.Data?.Objects?.OrderDetails, "OrderDetails", requireSingleResult: false);

            _logger.LogInformation("Order {OrderId} sent to Mahak successfully. Response: {Response}", 
                order.Id, responseText);

            // Mark order as synced
            order.SetMahakSynced(orderResult.EntityId.ToString());
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        private static string BuildOrderDescription(UserOrder order)
        {
            var shippingAddress = order.ShippingAddress ?? order.BillingAddress;
            if (shippingAddress == null)
            {
                return string.Empty;
            }

            var name = string.Join(" ", new[]
            {
                NormalizeShippingAddressValue(shippingAddress.FirstName),
                NormalizeShippingAddressValue(shippingAddress.LastName)
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

            var address = string.Join(" ", new[]
            {
                NormalizeShippingAddressValue(shippingAddress.State),
                NormalizeShippingAddressValue(shippingAddress.City),
                NormalizeShippingAddressValue(shippingAddress.AddressLine1),
                NormalizeShippingAddressValue(shippingAddress.AddressLine2)
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

            var phone = NormalizeShippingAddressValue(shippingAddress.PhoneNumber)
                ?? NormalizeShippingAddressValue(order.User?.PhoneNumber);

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                parts.Add(name);
            }

            if (!string.IsNullOrWhiteSpace(address))
            {
                parts.Add(address);
            }

            var postalCode = NormalizeShippingAddressValue(shippingAddress.PostalCode);
            if (!string.IsNullOrWhiteSpace(postalCode))
            {
                parts.Add($"کد پستی : {postalCode}");
            }

            parts.Add($"تماس : {phone}");

            return Environment.NewLine + string.Join(Environment.NewLine, parts);
        }

        private static string? BuildShippingAddressJson(UserOrder order)
        {
            return null;
        }

        private static string? NormalizeShippingAddressValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalizedWhitespace = string.Join(
                " ",
                value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

            return NormalizeDigits(normalizedWhitespace);
        }

        private static string NormalizeDigits(string value)
        {
            return value
                .Replace('۰', '0')
                .Replace('۱', '1')
                .Replace('۲', '2')
                .Replace('۳', '3')
                .Replace('۴', '4')
                .Replace('۵', '5')
                .Replace('۶', '6')
                .Replace('۷', '7')
                .Replace('۸', '8')
                .Replace('۹', '9')
                .Replace('٠', '0')
                .Replace('١', '1')
                .Replace('٢', '2')
                .Replace('٣', '3')
                .Replace('٤', '4')
                .Replace('٥', '5')
                .Replace('٦', '6')
                .Replace('٧', '7')
                .Replace('٨', '8')
                .Replace('٩', '9');
        }

        private static decimal GetItemDiscountAmount(UserOrderItem item)
        {
            if (item.DiscountAmount is > 0)
            {
                return item.DiscountAmount.Value;
            }

            var originalUnitPrice = item.Product?.Price ?? item.UnitPrice;
            return Math.Max(0m, originalUnitPrice - item.UnitPrice) * item.Quantity;
        }

        private static decimal GetOriginalUnitPrice(UserOrderItem item, decimal itemDiscount)
        {
            if (itemDiscount <= 0 || item.Quantity <= 0)
            {
                return item.UnitPrice;
            }

            return item.UnitPrice + (itemDiscount / item.Quantity);
        }

        private static string BuildOrderItemDescription(UserOrderItem item)
        {
            var parts = new List<string> { item.ProductName };

            if (!string.IsNullOrWhiteSpace(item.ProductVariant?.Color))
            {
                parts.Add(item.ProductVariant.Color);
            }

            if (!string.IsNullOrWhiteSpace(item.ProductVariant?.Size))
            {
                parts.Add(item.ProductVariant.Size);
            }

            return string.Join(" - ", parts);
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
            var personClientId = user.MahakPersonClientId ?? GetPositiveClientId(user.Id);
            var mahakPerson = new MahakPersonModel
            {
                PersonClientId = personClientId,
                PersonGroupId = personGroupId, // Required for creating person
                FirstName = user.FirstName,
                LastName = user.LastName,
                Mobile = user.PhoneNumber ?? string.Empty,
                Email = user.Email,
                PersonType = 0, // Real person
                Deleted = false
            };

            // Person must be created first so the real server-side PersonId can be used below.
            var personRequest = new SaveAllDataRequest
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
                        personRequest.Pictures = new List<MahakPictureModel> { pictureModel };
                        _logger.LogInformation("Including profile picture for user {UserId}", user.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to prepare profile picture for user {UserId}, continuing without it", user.Id);
                }
            }

            var personSaveResult = await SaveAllDataAsync(personRequest, $"customer {user.Id}", cancellationToken);
            var personResult = GetSuccessfulEntityResult(personSaveResult.Data?.Objects?.People, "People");
            if (personResult.EntityId <= 0)
            {
                throw new InvalidOperationException($"Mahak did not return a valid PersonId for user {user.Id}.");
            }

            // Create VisitorPeople link in a second request using the real PersonId.
            var visitorPersonClientId = personClientId * 10L + 1L;
            var visitorPerson = new MahakVisitorPersonModel
            {
                VisitorPersonClientId = visitorPersonClientId,
                VisitorId = _visitorId,
                PersonId = personResult.EntityId,
                PersonClientId = personClientId,
                Deleted = false
            };
            var visitorRequest = new SaveAllDataRequest
            {
                VisitorPeople = new List<MahakVisitorPersonModel> { visitorPerson }
            };
            var visitorSaveResult = await SaveAllDataAsync(visitorRequest, $"visitor-person for user {user.Id}", cancellationToken);
            GetSuccessfulEntityResult(visitorSaveResult.Data?.Objects?.VisitorPeople, "VisitorPeople");

            _logger.LogInformation(
                "Customer {UserId} synced to Mahak successfully. PersonId: {PersonId}, PersonClientId: {PersonClientId}, VisitorPersonClientId: {VisitorPersonClientId}",
                user.Id, personResult.EntityId, personClientId, visitorPersonClientId);

            user.MahakPersonId = personResult.EntityId;
            user.MahakPersonClientId = personClientId;
            user.MahakSyncedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to save Mahak identifiers for user {user.Id}: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }

            return personResult.EntityId;
        }

        private async Task<SaveAllDataResultApiResult> SaveAllDataAsync(
            SaveAllDataRequest request,
            string operation,
            CancellationToken cancellationToken)
        {
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogDebug("Sending {Operation} to Mahak. Request: {Request}", operation, requestJson);
            var comment = ResolveSaveAllDataComment(operation);
            await _mahakTrafficLogger.LogRequestAsync(
                operation,
                "SaveAllDataV2",
                comment.request,
                request,
                cancellationToken);

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json-patch+json");
            var response = await _httpClient.PostAsync("SaveAllDataV2", content, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            await _mahakTrafficLogger.LogResponseAsync(
                operation,
                "SaveAllDataV2",
                comment.response,
                (int)response.StatusCode,
                response.IsSuccessStatusCode,
                responseText,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Failed to send {operation} to Mahak. Status: {response.StatusCode}, Content: {responseText}");
            }

            return DeserializeSaveResult(responseText, operation);
        }

        private static (string request, string response) ResolveSaveAllDataComment(string operation)
        {
            if (operation.StartsWith("customer", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    "این دیتا برای ارسال مشتری به محک است",
                    "این دیتا پاسخ محک بعد از ارسال مشتری است");
            }

            if (operation.StartsWith("visitor-person", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    "این دیتا برای اتصال مشتری به ویزیتور محک است",
                    "این دیتا پاسخ محک بعد از اتصال مشتری به ویزیتور است");
            }

            return (
                "این دیتا برای ارسال اطلاعات به محک است",
                "این دیتا پاسخ محک بعد از ارسال اطلاعات است");
        }

        private static SaveAllDataResultApiResult DeserializeSaveResult(
            string responseText,
            string operation,
            bool throwOnRejected = true)
        {
            var result = JsonSerializer.Deserialize<SaveAllDataResultApiResult>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new InvalidOperationException($"Mahak returned an invalid response for {operation}: {responseText}");
            }

            if (throwOnRejected)
            {
                EnsureSaveResultAccepted(result, operation, responseText);
            }

            return result;
        }

        private static void EnsureSaveResultAccepted(
            SaveAllDataResultApiResult result,
            string operation,
            string responseText)
        {
            if (!result.Result)
            {
                throw new InvalidOperationException(
                    $"Mahak rejected {operation}. Code: {result.Code}, Message: {result.Message ?? responseText}");
            }
        }

        private static bool IsAlreadyRegisteredOrderResult(SaveAllDataResultApiResult result)
        {
            var orderResults = result.Data?.Objects?.Orders?.Results;
            if (orderResults == null || orderResults.Count == 0)
            {
                return false;
            }

            return orderResults.Any(orderResult =>
                !orderResult.Result &&
                (orderResult.Errors ?? new List<PropertyErrorModel>()).Any(error =>
                    string.Equals(error.Property, "OrderId", StringComparison.OrdinalIgnoreCase) &&
                    (error.Error?.Contains("\u0648\u06CC\u0631\u0627\u06CC\u0634", StringComparison.OrdinalIgnoreCase) ?? false) &&
                    (error.Error?.Contains("\u062B\u0628\u062A", StringComparison.OrdinalIgnoreCase) ?? false)));
        }

        private static EntityUpdateResult GetSuccessfulEntityResult(
            MultiEntityUpdateResult? updateResult,
            string entityName,
            bool requireSingleResult = true)
        {
            var results = updateResult?.Results;
            if (results == null || results.Count == 0)
            {
                throw new InvalidOperationException($"Mahak returned no result for {entityName}.");
            }

            var failed = results.Where(r => !r.Result).ToList();
            if (failed.Count > 0)
            {
                var errors = string.Join("; ", failed.SelectMany(r => r.Errors ?? new List<PropertyErrorModel>())
                    .Select(e => $"{e.Property}: {e.Error ?? e.Code.ToString()}"));
                throw new InvalidOperationException($"Mahak rejected {entityName}. {errors}".Trim());
            }

            if (requireSingleResult && results.Count != 1)
            {
                throw new InvalidOperationException($"Mahak returned {results.Count} results for {entityName}; expected one.");
            }

            return results[0];
        }

        private static long GetPositiveClientId(Guid id)
        {
            var value = id.GetHashCode();
            return value == int.MinValue ? (long)int.MaxValue + 1 : Math.Abs((long)value);
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

            await _mahakTrafficLogger.LogRequestAsync(
                "LoginOutgoing",
                "Login",
                "این دیتا برای ورود به محک قبل از ارسال مشتری یا فاکتور فروش است",
                loginModel,
                cancellationToken);

            var content = new StringContent(JsonSerializer.Serialize(loginModel), System.Text.Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var response = await _httpClient.PostAsync("Login", content, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            await _mahakTrafficLogger.LogResponseAsync(
                "LoginOutgoing",
                "Login",
                "این دیتا پاسخ محک برای ورود قبل از ارسال مشتری یا فاکتور فروش است",
                (int)response.StatusCode,
                response.IsSuccessStatusCode,
                responseText,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Mahak login failed. Status: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<MahakApiResult<LoginResultModel>>(
                responseText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
