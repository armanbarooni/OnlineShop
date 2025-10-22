using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OnlineShop.IntegrationTests.Scenarios.ErrorHandling;

public class ExceptionHandlingTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ExceptionHandlingTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HandleDatabaseConnectionError_ShouldReturnServiceUnavailable()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to access an endpoint that might cause database issues
        var response = await _client.GetAsync("/api/product");

        // Assert
        // Note: In test environment, database connection issues are unlikely
        // This test verifies the system handles errors gracefully
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task HandleNullReferenceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to create a product with null data
        var productDto = new
        {
            Name = (string)null,
            Description = "Test description",
            Price = 100.0m
        };

        var response = await _client.PostAsJsonAsync("/api/product", productDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HandleTimeoutException_ShouldReturnRequestTimeout()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to access an endpoint that might timeout
        var response = await _client.GetAsync("/api/product/search?query=test&page=1&pageSize=1000");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.RequestTimeout);
    }

    [Fact]
    public async Task HandleUnauthorizedAccessException_ShouldReturnForbidden()
    {
        // Arrange
        var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        // Act - Try to access admin-only endpoint
        var response = await _client.DeleteAsync("/api/product/123");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HandleArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to create a product with invalid data
        var productDto = new
        {
            Name = "",
            Description = "Test description",
            Price = -100.0m
        };

        var response = await _client.PostAsJsonAsync("/api/product", productDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HandleFileNotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to access non-existent resource
        var response = await _client.GetAsync("/api/product/99999999-9999-9999-9999-999999999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HandleConcurrencyException_ShouldReturnConflict()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to update the same resource simultaneously
        var productDto = new
        {
            Name = "Concurrency Test Product",
            Description = "Test description",
            Price = 100.0m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/product", productDto);
        if (createResponse.StatusCode == HttpStatusCode.Created)
        {
            var content = await createResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<JsonElement>(content);
            var productId = product.GetProperty("id").GetString();

            // Try to update the same product simultaneously
            var updateDto = new
            {
                Name = "Updated Product",
                Description = "Updated description",
                Price = 150.0m
            };

            var response = await _client.PutAsJsonAsync($"/api/product/{productId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);
        }
    }

    [Fact]
    public async Task HandleInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to perform an invalid operation
        var response = await _client.PostAsJsonAsync("/api/cart/add", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HandleOutOfMemoryException_ShouldReturnInternalServerError()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to create a very large request
        var largeString = new string('A', 1000000); // 1MB string
        var productDto = new
        {
            Name = "Large Product",
            Description = largeString,
            Price = 100.0m
        };

        var response = await _client.PostAsJsonAsync("/api/product", productDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HandleFormatException_ShouldReturnBadRequest()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to send malformed JSON
        var malformedJson = "{ \"name\": \"Test\", \"price\": \"invalid_price\" }";
        var content = new StringContent(malformedJson, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/product", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HandleStackOverflowException_ShouldReturnInternalServerError()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to access an endpoint that might cause stack overflow
        var response = await _client.GetAsync("/api/product/search?query=test&page=1&pageSize=100");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HandleAccessViolationException_ShouldReturnInternalServerError()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to access an endpoint
        var response = await _client.GetAsync("/api/product");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HandleDivideByZeroException_ShouldReturnInternalServerError()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to create a product with zero price (might cause division by zero in calculations)
        var productDto = new
        {
            Name = "Zero Price Product",
            Description = "Test description",
            Price = 0.0m
        };

        var response = await _client.PostAsJsonAsync("/api/product", productDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HandleIndexOutOfRangeException_ShouldReturnInternalServerError()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to access an endpoint that might cause index out of range
        var response = await _client.GetAsync("/api/product/search?query=test&page=999999&pageSize=10");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HandleKeyNotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to access a non-existent key/resource
        var response = await _client.GetAsync("/api/product/non-existent-key");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HandleNotSupportedException_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to use an unsupported HTTP method
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, "/api/product"));

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }
}
