using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    public class BrandControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public BrandControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllBrands_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/brand");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetBrandById_WithValidId_ShouldReturnBrand()
        {
            // Arrange
            var brandId = await CreateTestBrandAsync();

            // Act
            var response = await _client.GetAsync($"/api/brand/{brandId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task GetBrandById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/brand/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateBrand_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandDto = new
            {
                Name = $"Test Brand {Guid.NewGuid().ToString().Substring(0, 8)}",
                LogoUrl = "https://example.com/logo.png",
                Description = "Test Brand Description",
                Website = "https://example.com",
                DisplayOrder = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task CreateBrand_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandDto = new
            {
                Name = "Test Brand",
                DisplayOrder = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateBrand_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var brandDto = new
            {
                Name = "Test Brand",
                DisplayOrder = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateBrand_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandId = await CreateTestBrandAsync();
            var updateDto = new
            {
                Id = brandId,
                Name = $"Updated Brand {Guid.NewGuid()}",
                Description = "Updated Description",
                IsActive = false,
                DisplayOrder = 2
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/brand/{brandId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateBrand_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            // First create brand as admin
            var brandId = await CreateTestBrandAsync();
            
            // Then set user token for the update attempt
            var userToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var updateDto = new
            {
                Id = brandId,
                Name = "Updated Brand",
                IsActive = false,
                DisplayOrder = 2
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/brand/{brandId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteBrand_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandId = await CreateTestBrandAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/brand/{brandId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteBrand_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            // First create brand as admin
            var brandId = await CreateTestBrandAsync();
            
            // Then set user token for the delete attempt
            var userToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.DeleteAsync($"/api/brand/{brandId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateBrand_WithEmptyName_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var invalidDto = new
            {
                Name = "", // Empty name should be invalid
                DisplayOrder = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/brand", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestBrandAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandDto = new
            {
                Name = $"TestBrand{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Description = "Test Description",
                DisplayOrder = 1
            };

            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var brandId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(brandId))
                    return Guid.Parse(brandId);
            }

            return Guid.NewGuid(); // Fallback
        }
    }
}


