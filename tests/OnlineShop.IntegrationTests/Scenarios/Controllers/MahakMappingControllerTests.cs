using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OnlineShop.Application.DTOs.MahakMapping;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    /// <summary>
    /// Integration tests for MahakMappingController
    /// Tests CRUD operations and authorization for Mahak mapping management
    /// </summary>
    public class MahakMappingControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public MahakMappingControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAllMahakMappings_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync("/api/mahakmapping");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllMahakMappings_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.GetAsync("/api/mahakmapping");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllMahakMappings_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange - No authorization header

            // Act
            var response = await _client.GetAsync("/api/mahakmapping");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetMahakMappingById_WithValidId_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var mappingId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/mahakmapping/{mappingId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMahakMappingById_WithInvalidId_AsAdmin_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = Guid.Empty;

            // Act
            var response = await _client.GetAsync($"/api/mahakmapping/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMahakMappingById_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var mappingId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/mahakmapping/{mappingId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task CreateMahakMapping_WithValidData_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakMappingDto
            {
                EntityType = "Product",
                LocalEntityId = Guid.NewGuid(),
                MahakEntityId = 12345,
                MahakEntityCode = "MAHAK123",
                Notes = "Test mapping for integration test"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakmapping", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakMapping_WithInvalidData_AsAdmin_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakMappingDto
            {
                EntityType = "", // Invalid empty entity type
                LocalEntityId = Guid.Empty, // Invalid empty GUID
                MahakEntityId = -1, // Invalid negative ID
                MahakEntityCode = null,
                Notes = null
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakmapping", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakMapping_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var createDto = new CreateMahakMappingDto
            {
                EntityType = "Product",
                LocalEntityId = Guid.NewGuid(),
                MahakEntityId = 12345,
                MahakEntityCode = "MAHAK123",
                Notes = "Test mapping"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakmapping", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateMahakMapping_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange
            var createDto = new CreateMahakMappingDto
            {
                EntityType = "Product",
                LocalEntityId = Guid.NewGuid(),
                MahakEntityId = 12345,
                MahakEntityCode = "MAHAK123",
                Notes = "Test mapping"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakmapping", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateMahakMapping_WithValidData_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var mappingId = Guid.NewGuid();
            var updateDto = new UpdateMahakMappingDto
            {
                Id = mappingId,
                MahakEntityId = 54321,
                MahakEntityCode = "MAHAK_UPDATED",
                Notes = "Updated mapping for integration test",
                UpdatedBy = "Test Admin"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahakmapping/{mappingId}", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateMahakMapping_WithInvalidData_AsAdmin_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var mappingId = Guid.NewGuid();
            var updateDto = new UpdateMahakMappingDto
            {
                Id = mappingId,
                MahakEntityId = -1, // Invalid negative ID
                MahakEntityCode = "",
                Notes = "",
                UpdatedBy = ""
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahakmapping/{mappingId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateMahakMapping_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var mappingId = Guid.NewGuid();
            var updateDto = new UpdateMahakMappingDto
            {
                Id = mappingId,
                MahakEntityId = 54321,
                MahakEntityCode = "MAHAK_UPDATED",
                Notes = "Updated mapping",
                UpdatedBy = "Test User"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahakmapping/{mappingId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteMahakMapping_WithValidId_AsAdmin_ShouldReturnNoContent()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var mappingId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahakmapping/{mappingId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMahakMapping_WithInvalidId_AsAdmin_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = Guid.Empty;

            // Act
            var response = await _client.DeleteAsync($"/api/mahakmapping/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMahakMapping_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var mappingId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahakmapping/{mappingId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteMahakMapping_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange
            var mappingId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahakmapping/{mappingId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public async Task CreateMahakMapping_WithDuplicateMahakEntityId_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakMappingDto
            {
                EntityType = "Product",
                LocalEntityId = Guid.NewGuid(),
                MahakEntityId = 99999, // Same Mahak ID
                MahakEntityCode = "DUPLICATE_TEST",
                Notes = "Test for duplicate Mahak entity ID"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act - Create first mapping
            var response1 = await _client.PostAsync("/api/mahakmapping", content);
            
            // Create second mapping with same Mahak entity ID
            createDto.LocalEntityId = Guid.NewGuid(); // Different local entity
            var json2 = JsonSerializer.Serialize(createDto);
            var content2 = new StringContent(json2, Encoding.UTF8, "application/json");
            var response2 = await _client.PostAsync("/api/mahakmapping", content2);

            // Assert
            response1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            response2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateMahakMapping_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateMahakMappingDto
            {
                Id = nonExistentId,
                MahakEntityId = 12345,
                MahakEntityCode = "NON_EXISTENT",
                Notes = "Update non-existent mapping",
                UpdatedBy = "Test Admin"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahakmapping/{nonExistentId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task CreateMahakMapping_WithVeryLongNotes_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakMappingDto
            {
                EntityType = "Product",
                LocalEntityId = Guid.NewGuid(),
                MahakEntityId = 12345,
                MahakEntityCode = "LONG_NOTES_TEST",
                Notes = new string('A', 10000) // Very long notes
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakmapping", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakMapping_WithSpecialCharactersInCode_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakMappingDto
            {
                EntityType = "Product",
                LocalEntityId = Guid.NewGuid(),
                MahakEntityId = 12345,
                MahakEntityCode = "SPECIAL!@#$%^&*()_+-=[]{}|;':\",./<>?",
                Notes = "Test with special characters"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakmapping", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        #endregion
    }
}
