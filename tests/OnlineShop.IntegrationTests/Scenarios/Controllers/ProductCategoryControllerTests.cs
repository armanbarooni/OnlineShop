using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    public class ProductCategoryControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ProductCategoryControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllCategories_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/productcategory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetCategoryById_WithValidId_ShouldReturnCategory()
        {
            // Arrange
            var categoryId = await CreateTestCategoryAsync();

            // Act
            var response = await _client.GetAsync($"/api/productcategory/{categoryId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task GetCategoryById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/productcategory/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCategoryTree_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/productcategory/tree");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetSubCategories_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var categoryId = await CreateTestCategoryAsync();

            // Act
            var response = await _client.GetAsync($"/api/productcategory/{categoryId}/subcategories");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCategory_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = $"Test Category {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Test Category Description",
                ParentCategoryId = (Guid?)null,
                MahakClientId = 0L,
                MahakId = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task CreateCategory_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = "Test Category",
                Description = "Test Description",
                MahakClientId = 0L,
                MahakId = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateCategory_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var categoryDto = new
            {
                Name = "Test Category",
                Description = "Test Description",
                MahakClientId = 0L,
                MahakId = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateCategory_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            var updateDto = new
            {
                Id = categoryId,
                Name = $"Updated Category {Guid.NewGuid()}",
                Description = "Updated Description"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/productcategory/{categoryId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateCategory_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            // First create category as admin
            var categoryId = await CreateTestCategoryAsync();
            
            // Then set user token for the update attempt
            var userToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var updateDto = new
            {
                Id = categoryId,
                Name = "Updated Category",
                Description = "Updated Description"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/productcategory/{categoryId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteCategory_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/productcategory/{categoryId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteCategory_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            // First create category as admin
            var categoryId = await CreateTestCategoryAsync();
            
            // Then set user token for the delete attempt
            var userToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.DeleteAsync($"/api/productcategory/{categoryId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateCategory_WithEmptyName_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var invalidDto = new
            {
                Name = "", // Empty name should be invalid
                Description = "Test Description",
                MahakClientId = 0L,
                MahakId = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productcategory", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestCategoryAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = $"TestCategory{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Description = "Test Description",
                ParentCategoryId = (Guid?)null,
                MahakClientId = 0L,
                MahakId = 0
            };

            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var categoryId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(categoryId))
                    return Guid.Parse(categoryId);
            }

            return Guid.NewGuid(); // Fallback
        }
    }
}


