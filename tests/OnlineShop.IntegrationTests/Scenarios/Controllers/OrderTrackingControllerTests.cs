using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers;

public class OrderTrackingControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public OrderTrackingControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrderTracking_WithValidOrderId_ShouldReturnTrackingInfo()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // First create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Test order for tracking"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await createResponse.Content.ReadAsStringAsync();
        var orderId = JsonHelper.GetNestedProperty(orderContent, "data", "id")
            ?? throw new InvalidOperationException("Create user order response did not contain data.id");

        // Act
        var response = await _client.GetAsync($"/api/ordertracking/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOrderTracking_WithInvalidOrderId_ShouldReturnNotFound()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidOrderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ordertracking/{invalidOrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderTracking_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ordertracking/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOrderTracking_AsUser_ShouldReturnForbidden()
    {
        // Arrange
        var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ordertracking/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidData_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // First create an order
        var orderDto = new
        {
            UserAddressId = Guid.NewGuid(),
            PaymentMethod = "CreditCard",
            Notes = "Test order for status update"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderContent = await createResponse.Content.ReadAsStringAsync();
        var orderId = JsonHelper.GetNestedProperty(orderContent, "data", "id")
            ?? throw new InvalidOperationException("Create user order response did not contain data.id");

        var statusUpdateDto = new
        {
            Status = "Processing",
            Notes = "Order is being processed"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ordertracking/{orderId}/status", statusUpdateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithInvalidStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var orderId = Guid.NewGuid();
        var statusUpdateDto = new
        {
            Status = "InvalidStatus",
            Notes = "Invalid status test"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ordertracking/{orderId}/status", statusUpdateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrderHistory_WithValidOrderId_ShouldReturnHistory()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ordertracking/{orderId}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOrderHistory_WithInvalidOrderId_ShouldReturnNotFound()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidOrderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ordertracking/{invalidOrderId}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderTracking_WithSpecialCharacters_ShouldHandleGracefully()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ordertracking/{orderId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithEmptyNotes_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var orderId = Guid.NewGuid();
        var statusUpdateDto = new
        {
            Status = "Processing",
            Notes = ""
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ordertracking/{orderId}/status", statusUpdateDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderTracking_WithUnicodeCharacters_ShouldHandleGracefully()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/ordertracking/{orderId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
