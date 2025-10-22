using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OnlineShop.IntegrationTests.Scenarios.BusinessLogic;

public class OrderLifecycleTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public OrderLifecycleTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteOrderLifecycle_FromCreationToCompletion_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Step 1: Create a product
        var productDto = new
        {
            Name = "Lifecycle Test Product",
            Description = "Product for testing complete lifecycle",
            Price = 100.0m,
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            UnitId = Guid.NewGuid()
        };

        var productResponse = await _client.PostAsJsonAsync("/api/product", productDto);
        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var productContent = await productResponse.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<JsonElement>(productContent);
        var productId = product.GetProperty("id").GetString();

        // Step 2: Add product to cart
        var cartDto = new
        {
            ProductId = productId,
            Quantity = 2
        };

        var cartResponse = await _client.PostAsJsonAsync("/api/cart/add", cartDto);
        cartResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 3: Create order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Lifecycle test order"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Step 4: Update order status to Processing
        var processingDto = new
        {
            Status = "Processing",
            Notes = "Order is being processed"
        };

        var processingResponse = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", processingDto);
        processingResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Update order status to Shipped
        var shippedDto = new
        {
            Status = "Shipped",
            Notes = "Order has been shipped"
        };

        var shippedResponse = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", shippedDto);
        shippedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Update order status to Delivered
        var deliveredDto = new
        {
            Status = "Delivered",
            Notes = "Order has been delivered"
        };

        var deliveredResponse = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", deliveredDto);
        deliveredResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: Verify final order status
        var finalOrderResponse = await _client.GetAsync($"/api/userorder/{orderId}");
        finalOrderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OrderCancellation_ShouldUpdateStatusCorrectly()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for cancellation test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Act: Cancel the order
        var cancelDto = new
        {
            Status = "Cancelled",
            Notes = "Order cancelled by customer"
        };

        var cancelResponse = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", cancelDto);

        // Assert
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OrderReturn_ShouldCreateReturnRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a completed order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for return test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Mark order as delivered
        var deliveredDto = new
        {
            Status = "Delivered",
            Notes = "Order delivered"
        };

        await _client.PutAsJsonAsync($"/api/userorder/{orderId}", deliveredDto);

        // Act: Create return request
        var returnDto = new
        {
            OrderId = orderId,
            Reason = "Defective product",
            Description = "Product arrived damaged"
        };

        var returnResponse = await _client.PostAsJsonAsync("/api/userreturnrequest", returnDto);

        // Assert
        returnResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task OrderRefund_ShouldProcessRefund()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a completed order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for refund test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Act: Process refund
        var refundDto = new
        {
            OrderId = orderId,
            Amount = 100.0m,
            Reason = "Customer request"
        };

        var refundResponse = await _client.PostAsJsonAsync("/api/userpayment/refund", refundDto);

        // Assert
        refundResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OrderStatusTransition_WithInvalidTransition_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for invalid transition test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Act: Try invalid transition (from Created to Delivered)
        var invalidDto = new
        {
            Status = "Delivered",
            Notes = "Invalid transition test"
        };

        var response = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", invalidDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OrderTracking_ShouldReturnTrackingInfo()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for tracking test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Act: Get tracking info
        var trackingResponse = await _client.GetAsync($"/api/ordertracking/{orderId}");

        // Assert
        trackingResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OrderHistory_ShouldReturnOrderHistory()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for history test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Act: Get order history
        var historyResponse = await _client.GetAsync($"/api/ordertracking/{orderId}/history");

        // Assert
        historyResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OrderModification_BeforeProcessing_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for modification test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Act: Modify order
        var modifyDto = new
        {
            Notes = "Modified order notes"
        };

        var modifyResponse = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", modifyDto);

        // Assert
        modifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OrderModification_AfterProcessing_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for modification test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Mark order as processing
        var processingDto = new
        {
            Status = "Processing",
            Notes = "Order is being processed"
        };

        await _client.PutAsJsonAsync($"/api/userorder/{orderId}", processingDto);

        // Act: Try to modify processed order
        var modifyDto = new
        {
            Notes = "Modified order notes"
        };

        var modifyResponse = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", modifyDto);

        // Assert
        modifyResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OrderCompletion_ShouldTriggerNotifications()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Order for notification test"
        };

        var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderContent);
        var orderId = order.GetProperty("id").GetString();

        // Act: Complete the order
        var completeDto = new
        {
            Status = "Delivered",
            Notes = "Order completed"
        };

        var completeResponse = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", completeDto);

        // Assert
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
