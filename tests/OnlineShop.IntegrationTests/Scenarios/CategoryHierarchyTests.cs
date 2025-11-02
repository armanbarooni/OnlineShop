using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class CategoryHierarchyTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public CategoryHierarchyTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetCategoryTree_ShouldReturnHierarchicalStructure()
        {
            // Act
            var response = await _client.GetAsync("/api/productcategory/tree");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetSubCategories_WithParentId_ShouldReturnSubCategories()
        {
            // Arrange - Create parent category
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var parentCategory = new CreateProductCategoryDto
            {
                Name = "Parent Category",
                Description = "Parent",
                MahakClientId = 1,
                MahakId = 1,
                ParentCategoryId = null
            };

            var createResponse = await _client.PostAsJsonAsync("/api/productcategory", parentCategory);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await createResponse.Content.ReadAsStringAsync();
            var parentId = JsonHelper.GetNestedProperty(responseContent, "data", "id");

            if (string.IsNullOrEmpty(parentId))
            {
                Console.WriteLine($"[CategoryHierarchyTests] Failed to extract parent ID from: {responseContent}");
                throw new Exception($"Failed to extract parent ID from response: {responseContent}");
            }

            // Act - Get subcategories
            var response = await _client.GetAsync($"/api/productcategory/{parentId}/subcategories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateCategory_WithParentId_ShouldCreateSubCategory()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Create parent
            var parentCategory = new CreateProductCategoryDto
            {
                Name = "Electronics",
                Description = "Electronics Category",
                MahakClientId = 100,
                MahakId = 100,
                ParentCategoryId = null
            };

            var parentResponse = await _client.PostAsJsonAsync("/api/productcategory", parentCategory);
            var parentContent = await parentResponse.Content.ReadAsStringAsync();
            var parentIdStr = JsonHelper.GetNestedProperty(parentContent, "data", "id");

            if (string.IsNullOrEmpty(parentIdStr))
            {
                Console.WriteLine($"[CategoryHierarchyTests] Failed to extract parent ID from: {parentContent}");
                throw new Exception($"Failed to extract parent ID from response: {parentContent}");
            }

            var parentId = Guid.Parse(parentIdStr);

            // Act - Create subcategory
            var subCategory = new CreateProductCategoryDto
            {
                Name = "Laptops",
                Description = "Laptop Subcategory",
                MahakClientId = 101,
                MahakId = 101,
                ParentCategoryId = parentId
            };

            var subResponse = await _client.PostAsJsonAsync("/api/productcategory", subCategory);

            // Assert
            subResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            var subContent = await subResponse.Content.ReadAsStringAsync();
            var subId = JsonHelper.GetNestedProperty(subContent, "data", "id");
            subId.Should().NotBeNullOrEmpty();
        }
    }
}

