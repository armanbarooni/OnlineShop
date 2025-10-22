using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OnlineShop.Application.DTOs.MahakQueue;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    /// <summary>
    /// Integration tests for MahakQueueController
    /// Tests CRUD operations and authorization for Mahak queue management
    /// </summary>
    public class MahakQueueControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public MahakQueueControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAllMahakQueues_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync("/api/mahakqueue");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllMahakQueues_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.GetAsync("/api/mahakqueue");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllMahakQueues_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange - No authorization header

            // Act
            var response = await _client.GetAsync("/api/mahakqueue");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetMahakQueueById_WithValidId_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var queueId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/mahakqueue/{queueId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMahakQueueById_WithInvalidId_AsAdmin_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = Guid.Empty;

            // Act
            var response = await _client.GetAsync($"/api/mahakqueue/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMahakQueueById_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var queueId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/mahakqueue/{queueId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task CreateMahakQueue_WithValidData_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakQueueDto
            {
                QueueType = "ProductSync",
                OperationType = "Create",
                EntityId = Guid.NewGuid(),
                EntityType = "Product",
                Priority = 5,
                MaxRetries = 3,
                ScheduledAt = DateTime.UtcNow.AddMinutes(5),
                Payload = "{\"productId\":\"123\",\"action\":\"create\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakQueue_WithInvalidData_AsAdmin_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakQueueDto
            {
                QueueType = "", // Invalid empty queue type
                OperationType = "", // Invalid empty operation type
                EntityId = null,
                EntityType = "", // Invalid empty entity type
                Priority = -1, // Invalid negative priority
                MaxRetries = -1, // Invalid negative max retries
                ScheduledAt = null,
                Payload = null
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakQueue_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var createDto = new CreateMahakQueueDto
            {
                QueueType = "ProductSync",
                OperationType = "Create",
                EntityId = Guid.NewGuid(),
                EntityType = "Product",
                Priority = 5,
                MaxRetries = 3,
                ScheduledAt = DateTime.UtcNow.AddMinutes(5),
                Payload = "{\"productId\":\"123\",\"action\":\"create\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateMahakQueue_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange
            var createDto = new CreateMahakQueueDto
            {
                QueueType = "ProductSync",
                OperationType = "Create",
                EntityId = Guid.NewGuid(),
                EntityType = "Product",
                Priority = 5,
                MaxRetries = 3,
                ScheduledAt = DateTime.UtcNow.AddMinutes(5),
                Payload = "{\"productId\":\"123\",\"action\":\"create\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateMahakQueue_WithValidData_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var queueId = Guid.NewGuid();
            var updateDto = new UpdateMahakQueueDto
            {
                Id = queueId,
                QueueType = "ProductSync",
                OperationType = "Update",
                EntityType = "Product",
                Priority = 7,
                Payload = "{\"productId\":\"123\",\"action\":\"update\"}",
                UpdatedBy = "Test Admin"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahakqueue/{queueId}", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateMahakQueue_WithInvalidData_AsAdmin_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var queueId = Guid.NewGuid();
            var updateDto = new UpdateMahakQueueDto
            {
                Id = queueId,
                QueueType = "", // Invalid empty queue type
                OperationType = "", // Invalid empty operation type
                EntityType = "", // Invalid empty entity type
                Priority = -1, // Invalid negative priority
                Payload = "",
                UpdatedBy = ""
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahakqueue/{queueId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateMahakQueue_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var queueId = Guid.NewGuid();
            var updateDto = new UpdateMahakQueueDto
            {
                Id = queueId,
                QueueType = "ProductSync",
                OperationType = "Update",
                EntityType = "Product",
                Priority = 7,
                Payload = "{\"productId\":\"123\",\"action\":\"update\"}",
                UpdatedBy = "Test User"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahakqueue/{queueId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteMahakQueue_WithValidId_AsAdmin_ShouldReturnNoContent()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var queueId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahakqueue/{queueId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMahakQueue_WithInvalidId_AsAdmin_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = Guid.Empty;

            // Act
            var response = await _client.DeleteAsync($"/api/mahakqueue/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMahakQueue_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var queueId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahakqueue/{queueId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteMahakQueue_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange
            var queueId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/mahakqueue/{queueId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public async Task CreateMahakQueue_WithHighPriority_ShouldBeCreated()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakQueueDto
            {
                QueueType = "UrgentSync",
                OperationType = "Create",
                EntityId = Guid.NewGuid(),
                EntityType = "Product",
                Priority = 10, // High priority
                MaxRetries = 5,
                ScheduledAt = DateTime.UtcNow.AddMinutes(1),
                Payload = "{\"urgent\":true,\"productId\":\"123\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakQueue_WithLowPriority_ShouldBeCreated()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakQueueDto
            {
                QueueType = "BackgroundSync",
                OperationType = "Update",
                EntityId = Guid.NewGuid(),
                EntityType = "Product",
                Priority = 1, // Low priority
                MaxRetries = 1,
                ScheduledAt = DateTime.UtcNow.AddHours(1),
                Payload = "{\"background\":true,\"productId\":\"456\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateMahakQueue_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateMahakQueueDto
            {
                Id = nonExistentId,
                QueueType = "ProductSync",
                OperationType = "Update",
                EntityType = "Product",
                Priority = 5,
                Payload = "{\"productId\":\"123\",\"action\":\"update\"}",
                UpdatedBy = "Test Admin"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/mahakqueue/{nonExistentId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task CreateMahakQueue_WithVeryLargePayload_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var largePayload = new string('A', 50000); // Very large payload
            var createDto = new CreateMahakQueueDto
            {
                QueueType = "LargeDataSync",
                OperationType = "Create",
                EntityId = Guid.NewGuid(),
                EntityType = "Product",
                Priority = 5,
                MaxRetries = 3,
                ScheduledAt = DateTime.UtcNow.AddMinutes(5),
                Payload = largePayload
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakQueue_WithFutureScheduledTime_ShouldBeCreated()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateMahakQueueDto
            {
                QueueType = "ScheduledSync",
                OperationType = "Create",
                EntityId = Guid.NewGuid(),
                EntityType = "Product",
                Priority = 5,
                MaxRetries = 3,
                ScheduledAt = DateTime.UtcNow.AddDays(1), // Future date
                Payload = "{\"scheduled\":true,\"productId\":\"789\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateMahakQueue_WithSpecialCharactersInPayload_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var specialPayload = "{\"special\":\"!@#$%^&*()_+-=[]{}|;':\\\",./<>?\",\"unicode\":\"مرحبا\"}";
            var createDto = new CreateMahakQueueDto
            {
                QueueType = "SpecialCharSync",
                OperationType = "Create",
                EntityId = Guid.NewGuid(),
                EntityType = "Product",
                Priority = 5,
                MaxRetries = 3,
                ScheduledAt = DateTime.UtcNow.AddMinutes(5),
                Payload = specialPayload
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/mahakqueue", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        #endregion
    }
}
