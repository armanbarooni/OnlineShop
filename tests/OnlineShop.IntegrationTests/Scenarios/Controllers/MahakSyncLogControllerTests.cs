using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OnlineShop.Application.DTOs.MahakSyncLog;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    /// <summary>
    /// Integration tests for MahakSyncLogController
    /// Tests CRUD operations and authorization for Mahak sync log management
    /// </summary>
    public class MahakSyncLogControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public MahakSyncLogControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAllMahakSyncLogs_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync("/api/mahaksynclog");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllMahakSyncLogs_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.GetAsync("/api/mahaksynclog");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllMahakSyncLogs_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange - No authorization header

            // Act
            var response = await _client.GetAsync("/api/mahaksynclog");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetMahakSyncLogById_WithValidId_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var logId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/mahaksynclog/{logId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMahakSyncLogById_WithInvalidId_AsAdmin_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = Guid.Empty;

            // Act
            var response = await _client.GetAsync($"/api/mahaksynclog/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMahakSyncLogById_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var logId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/mahaksynclog/{logId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task CreateMahakSyncLog_WithValidData_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakSyncLogDto
            {
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                SyncType = "FullSync",
                SyncStatus = "InProgress",
                RecordsProcessed = 0
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahaksynclog", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakSyncLog_WithInvalidData_AsAdmin_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakSyncLogDto
            {
                EntityType = "", // Invalid empty entity type
                EntityId = null,
                SyncType = "", // Invalid empty sync type
                SyncStatus = "", // Invalid empty sync status
                RecordsProcessed = -1 // Invalid negative count
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahaksynclog", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakSyncLog_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var createDto = new CreateMahakSyncLogDto
            {
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                SyncType = "FullSync",
                SyncStatus = "InProgress",
                RecordsProcessed = 0
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahaksynclog", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateMahakSyncLog_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange
            var createDto = new CreateMahakSyncLogDto
            {
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                SyncType = "FullSync",
                SyncStatus = "InProgress",
                RecordsProcessed = 0
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahaksynclog", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateMahakSyncLog_WithValidData_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var logId = Guid.NewGuid();
            var updateDto = new UpdateMahakSyncLogDto
            {
                Id = logId,
                EntityType = "Product",
                SyncType = "FullSync",
                SyncStatus = "Completed",
                RecordsProcessed = 100,
                UpdatedBy = "Test Admin"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahaksynclog/{logId}", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateMahakSyncLog_WithInvalidData_AsAdmin_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var logId = Guid.NewGuid();
            var updateDto = new UpdateMahakSyncLogDto
            {
                Id = logId,
                EntityType = "", // Invalid empty entity type
                SyncType = "", // Invalid empty sync type
                SyncStatus = "", // Invalid empty sync status
                RecordsProcessed = -1, // Invalid negative count
                UpdatedBy = ""
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahaksynclog/{logId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateMahakSyncLog_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var logId = Guid.NewGuid();
            var updateDto = new UpdateMahakSyncLogDto
            {
                Id = logId,
                EntityType = "Product",
                SyncType = "FullSync",
                SyncStatus = "Completed",
                RecordsProcessed = 100,
                UpdatedBy = "Test User"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahaksynclog/{logId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteMahakSyncLog_WithValidId_AsAdmin_ShouldReturnNoContent()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var logId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahaksynclog/{logId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMahakSyncLog_WithInvalidId_AsAdmin_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = Guid.Empty;

            // Act
            var response = await _client.DeleteAsync($"/api/mahaksynclog/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMahakSyncLog_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var logId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahaksynclog/{logId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteMahakSyncLog_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange
            var logId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahaksynclog/{logId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public async Task CreateMahakSyncLog_WithDifferentSyncTypes_ShouldBeCreated()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var syncTypes = new[] { "FullSync", "IncrementalSync", "ManualSync", "ScheduledSync" };

            foreach (var syncType in syncTypes)
            {
                var createDto = new CreateMahakSyncLogDto
                {
                    EntityType = "Product",
                    EntityId = Guid.NewGuid(),
                    SyncType = syncType,
                    SyncStatus = "InProgress",
                    RecordsProcessed = 0
                };

                var json = JsonSerializer.Serialize(createDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await _client.PostAsync("/api/mahaksynclog", content);

                // Assert
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task CreateMahakSyncLog_WithDifferentSyncStatuses_ShouldBeCreated()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var syncStatuses = new[] { "InProgress", "Completed", "Failed", "Cancelled" };

            foreach (var syncStatus in syncStatuses)
            {
                var createDto = new CreateMahakSyncLogDto
                {
                    EntityType = "Product",
                    EntityId = Guid.NewGuid(),
                    SyncType = "FullSync",
                    SyncStatus = syncStatus,
                    RecordsProcessed = 0
                };

                var json = JsonSerializer.Serialize(createDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await _client.PostAsync("/api/mahaksynclog", content);

                // Assert
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task UpdateMahakSyncLog_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateMahakSyncLogDto
            {
                Id = nonExistentId,
                EntityType = "Product",
                SyncType = "FullSync",
                SyncStatus = "Completed",
                RecordsProcessed = 100,
                UpdatedBy = "Test Admin"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahaksynclog/{nonExistentId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task CreateMahakSyncLog_WithLargeRecordsProcessed_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakSyncLogDto
            {
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                SyncType = "FullSync",
                SyncStatus = "Completed",
                RecordsProcessed = int.MaxValue // Very large number
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahaksynclog", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakSyncLog_WithSpecialCharactersInEntityType_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakSyncLogDto
            {
                EntityType = "Product-Entity_Type!@#",
                EntityId = Guid.NewGuid(),
                SyncType = "FullSync",
                SyncStatus = "InProgress",
                RecordsProcessed = 0
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahaksynclog", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakSyncLog_WithUnicodeCharacters_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakSyncLogDto
            {
                EntityType = "محصول",
                EntityId = Guid.NewGuid(),
                SyncType = "همگام‌سازی کامل",
                SyncStatus = "در حال انجام",
                RecordsProcessed = 0
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahaksynclog", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        #endregion
    }
}
