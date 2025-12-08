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
    public class MahakOutgoingSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MahakOutgoingSyncService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserOrderRepository _orderRepository;
        private readonly IMahakMappingRepository _mahakMappingRepository;
        private readonly IMahakSyncLogRepository _mahakSyncLogRepository;

        private string? _token;
        private int _visitorId;
        private const string BaseUrl = "https://mahakacc.mahaksoft.com/API/v3/Sync/";

        public MahakOutgoingSyncService(
            HttpClient httpClient,
            ILogger<MahakOutgoingSyncService> logger,
            IConfiguration configuration,
            IUserOrderRepository orderRepository,
            IMahakMappingRepository mahakMappingRepository,
            IMahakSyncLogRepository mahakSyncLogRepository)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _orderRepository = orderRepository;
            _mahakMappingRepository = mahakMappingRepository;
            _mahakSyncLogRepository = mahakSyncLogRepository;
            
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task SyncOrdersToMahakAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting outgoing sync to Mahak...");

                // 1. Login
                if (string.IsNullOrEmpty(_token))
                {
                    await LoginAsync(cancellationToken);
                }

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

        private async Task SendOrderToMahakAsync(UserOrder order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending order {OrderId} to Mahak", order.Id);

            // Convert order to Mahak format
            var mahakOrder = new MahakOrderModel
            {
                OrderClientId = order.Id.GetHashCode(), // Use hash of GUID as long
                VisitorId = _visitorId,
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
                ShippingAddress = order.ShippingAddress?.ToString()
            };

            var mahakOrderDetails = new List<MahakOrderDetailModel>();

            // Convert order items
            foreach (var item in order.OrderItems)
            {
                // Find product's Mahak ID
                var productMapping = await _mahakMappingRepository.GetByLocalEntityIdAsync(
                    "Product",
                    item.ProductId,
                    cancellationToken);

                if (productMapping == null)
                {
                    _logger.LogWarning("Product {ProductId} not found in Mahak mapping, skipping", item.ProductId);
                    continue;
                }

                mahakOrderDetails.Add(new MahakOrderDetailModel
                {
                    OrderDetailClientId = item.Id.GetHashCode(),
                    ItemType = 0, // Product
                    OrderClientId = mahakOrder.OrderClientId,
                    ProductDetailId = productMapping.MahakEntityId,
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

            // Send to Mahak
            var request = new SaveAllDataRequest
            {
                Orders = new List<MahakOrderModel> { mahakOrder },
                OrderDetails = mahakOrderDetails
            };

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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            _logger.LogInformation("Mahak outgoing sync login successful. VisitorId: {VisitorId}", _visitorId);
        }
    }
}
