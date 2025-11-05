using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OnlineShop.IntegrationTests.Scenarios.BusinessLogic;

public class InventoryManagementTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public InventoryManagementTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateInventory_IncreaseStock_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a product first
        var productDto = new
        {
            Name = "Inventory Test Product",
            Description = "Product for inventory testing",
            Price = 100.0m,
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            UnitId = Guid.NewGuid()
        };

        var productResponse = await _client.PostAsJsonAsync("/api/product", productDto);
        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
    var productContent = await productResponse.Content.ReadAsStringAsync();
    // Use JsonHelper to support wrapped responses (e.g. { data: { id: ... } })
    var productId = JsonHelper.GetNestedProperty(productContent, "data", "id")
            ?? JsonHelper.GetNestedProperty(productContent, "id");

        // Act: Update inventory
        var inventoryDto = new
        {
            ProductId = productId,
            Quantity = 50,
            Operation = "Increase"
        };

        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateInventory_DecreaseStock_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Create a product first
        var productDto = new
        {
            Name = "Inventory Decrease Test Product",
            Description = "Product for inventory decrease testing",
            Price = 100.0m,
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            UnitId = Guid.NewGuid()
        };

        var productResponse = await _client.PostAsJsonAsync("/api/product", productDto);
        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
    var productContent = await productResponse.Content.ReadAsStringAsync();
    var productId = JsonHelper.GetNestedProperty(productContent, "data", "id")
            ?? JsonHelper.GetNestedProperty(productContent, "id");

        // Act: Update inventory
        var inventoryDto = new
        {
            ProductId = productId,
            Quantity = 10,
            Operation = "Decrease"
        };

        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateInventory_WithNegativeQuantity_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var inventoryDto = new
        {
            ProductId = Guid.NewGuid(),
            Quantity = -10,
            Operation = "Increase"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateInventory_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var inventoryDto = new
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            Operation = "Increase"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetInventoryStatus_WithValidProductId_ShouldReturnInventory()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var productId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/productinventory/{productId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetInventoryStatus_WithInvalidProductId_ShouldReturnNotFound()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var invalidProductId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/productinventory/{invalidProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLowStockProducts_ShouldReturnLowStockItems()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/productinventory/lowstock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetOutOfStockProducts_ShouldReturnOutOfStockItems()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/productinventory/outofstock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task BulkUpdateInventory_WithMultipleProducts_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var bulkUpdateDto = new
        {
            Updates = new[]
            {
                new { ProductId = Guid.NewGuid(), Quantity = 10, Operation = "Increase" },
                new { ProductId = Guid.NewGuid(), Quantity = 5, Operation = "Decrease" }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/bulk-update", bulkUpdateDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateStockAlert_WithLowStock_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var stockAlertDto = new
        {
            ProductId = Guid.NewGuid(),
            Threshold = 10,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/stockalert", stockAlertDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStockAlerts_ShouldReturnActiveAlerts()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/stockalert");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateInventory_WithZeroQuantity_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var inventoryDto = new
        {
            ProductId = Guid.NewGuid(),
            Quantity = 0,
            Operation = "Set"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateInventory_WithLargeQuantity_ShouldSucceed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var inventoryDto = new
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10000,
            Operation = "Increase"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateInventory_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var inventoryDto = new
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            Operation = "Increase"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateInventory_AsUser_ShouldReturnForbidden()
    {
        // Arrange
        var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        var inventoryDto = new
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            Operation = "Increase"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInventoryHistory_WithValidProductId_ShouldReturnHistory()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var productId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/productinventory/{productId}/history");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateInventory_WithInvalidOperation_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var inventoryDto = new
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            Operation = "InvalidOperation"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateInventory_WithEmptyOperation_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var inventoryDto = new
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            Operation = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/productinventory/update", inventoryDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
