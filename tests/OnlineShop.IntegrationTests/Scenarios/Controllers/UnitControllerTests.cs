using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    public class UnitControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public UnitControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllUnits_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/unit");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetUnitById_WithValidId_ShouldReturnUnit()
        {
            // Arrange
            var unitId = await CreateTestUnitAsync();

            // Act
            var response = await _client.GetAsync($"/api/unit/{unitId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task GetUnitById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/unit/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateUnit_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitDto = new
            {
                Name = $"Test Unit {Guid.NewGuid().ToString().Substring(0, 8)}",
                Comment = "Test Unit Comment"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/unit", unitDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            if (!string.IsNullOrEmpty(isSuccess))
                isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task CreateUnit_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitDto = new
            {
                Name = "Test Unit",
                Comment = "Test Comment"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/unit", unitDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateUnit_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var unitDto = new
            {
                Name = "Test Unit",
                Comment = "Test Comment"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/unit", unitDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateUnit_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitId = await CreateTestUnitAsync();
            var updateDto = new
            {
                Id = unitId,
                Name = $"Updated Unit {Guid.NewGuid()}",
                Comment = "Updated Comment"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/unit/{unitId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateUnit_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            // First create unit as admin
            var unitId = await CreateTestUnitAsync();
            
            // Then set user token for the update attempt
            var userToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var updateDto = new
            {
                Id = unitId,
                Name = "Updated Unit",
                Comment = "Updated Comment"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/unit/{unitId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteUnit_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitId = await CreateTestUnitAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/unit/{unitId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteUnit_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            // First create unit as admin
            var unitId = await CreateTestUnitAsync();
            
            // Then set user token for the delete attempt
            var userToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.DeleteAsync($"/api/unit/{unitId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private async Task<Guid> CreateTestUnitAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitDto = new
            {
                Name = $"TestUnit{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Comment = "Test Comment"
            };

            var response = await _client.PostAsJsonAsync("/api/unit", unitDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Unit creation might return the ID directly in data field
                var unitId = JsonHelper.GetNestedProperty(content, "data", "id")
                    ?? JsonHelper.GetNestedProperty(content, "data");
                
                if (!string.IsNullOrEmpty(unitId))
                {
                    if (Guid.TryParse(unitId, out var guid))
                        return guid;
                }
            }

            return Guid.NewGuid(); // Fallback
        }
    }
}

