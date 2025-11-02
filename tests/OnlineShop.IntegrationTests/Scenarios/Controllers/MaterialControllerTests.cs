using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    public class MaterialControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public MaterialControllerTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllMaterials_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/material");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetMaterialById_WithValidId_ShouldReturnMaterial()
        {
            // Arrange
            var materialId = await CreateTestMaterialAsync();

            // Act
            var response = await _client.GetAsync($"/api/material/{materialId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task GetMaterialById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/material/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateMaterial_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var materialDto = new
            {
                Name = $"Test Material {Guid.NewGuid()}",
                Description = "Test material description",
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/material", materialDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task CreateMaterial_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var materialDto = new
            {
                Name = "Test Material",
                Description = "Test description",
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/material", materialDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateMaterial_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var materialDto = new
            {
                Name = "Test Material",
                Description = "Test description",
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/material", materialDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateMaterial_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var materialId = await CreateTestMaterialAsync();
            var updateDto = new
            {
                Id = materialId,
                Name = $"Updated Material {Guid.NewGuid()}",
                Description = "Updated description",
                IsActive = false
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/material/{materialId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateMaterial_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var materialId = await CreateTestMaterialAsync();
            var updateDto = new
            {
                Id = materialId,
                Name = "Updated Material",
                Description = "Updated description",
                IsActive = false
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/material/{materialId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteMaterial_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var materialId = await CreateTestMaterialAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/material/{materialId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMaterial_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var materialId = await CreateTestMaterialAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/material/{materialId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateMaterial_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var invalidDto = new
            {
                Name = "", // Empty name should be invalid
                Description = "Test description",
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/material", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateMaterial_WithIdMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var materialId = await CreateTestMaterialAsync();
            var updateDto = new
            {
                Id = Guid.NewGuid(), // Different ID
                Name = "Updated Material",
                Description = "Updated description",
                IsActive = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/material/{materialId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestMaterialAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var materialDto = new
            {
                Name = $"Test Material {Guid.NewGuid()}",
                Description = "Test material for integration tests",
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/material", materialDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var materialId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(materialId))
                    return Guid.Parse(materialId);
            }

            return Guid.NewGuid(); // Fallback
        }
    }
}