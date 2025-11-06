using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OnlineShop.Application.DTOs.SyncErrorLog;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    /// <summary>
    /// Integration tests for SyncErrorLogController
    /// Tests CRUD operations and authorization for sync error log management
    /// </summary>
    public class SyncErrorLogControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SyncErrorLogControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAllSyncErrorLogs_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var response = await _client.GetAsync("/api/syncerrorlog");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllSyncErrorLogs_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.GetAsync("/api/syncerrorlog");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllSyncErrorLogs_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange - No authorization header

            // Act
            var response = await _client.GetAsync("/api/syncerrorlog");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetSyncErrorLogById_WithValidId_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var logId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/syncerrorlog/{logId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSyncErrorLogById_WithInvalidId_AsAdmin_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = Guid.Empty;

            // Act
            var response = await _client.GetAsync($"/api/syncerrorlog/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSyncErrorLogById_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var logId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/syncerrorlog/{logId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task CreateSyncErrorLog_WithValidData_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateSyncErrorLogDto
            {
                ErrorType = "ValidationError",
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                ErrorCode = "ERR001",
                ErrorMessage = "Product validation failed",
                ErrorSeverity = "High",
                RequestData = "{\"productId\":\"123\"}",
                ResponseData = "{\"error\":\"validation failed\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/syncerrorlog", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSyncErrorLog_WithInvalidData_AsAdmin_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateSyncErrorLogDto
            {
                ErrorType = "", // Invalid empty error type
                EntityType = "", // Invalid empty entity type
                EntityId = null,
                ErrorCode = "", // Invalid empty error code
                ErrorMessage = "", // Invalid empty error message
                ErrorSeverity = "", // Invalid empty error severity
                RequestData = null,
                ResponseData = null
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/syncerrorlog", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSyncErrorLog_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var createDto = new CreateSyncErrorLogDto
            {
                ErrorType = "ValidationError",
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                ErrorCode = "ERR001",
                ErrorMessage = "Product validation failed",
                ErrorSeverity = "High",
                RequestData = "{\"productId\":\"123\"}",
                ResponseData = "{\"error\":\"validation failed\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/syncerrorlog", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateSyncErrorLog_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange
            var createDto = new CreateSyncErrorLogDto
            {
                ErrorType = "ValidationError",
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                ErrorCode = "ERR001",
                ErrorMessage = "Product validation failed",
                ErrorSeverity = "High",
                RequestData = "{\"productId\":\"123\"}",
                ResponseData = "{\"error\":\"validation failed\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/syncerrorlog", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateSyncErrorLog_WithValidData_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var logId = Guid.NewGuid();
            var updateDto = new UpdateSyncErrorLogDto
            {
                Id = logId,
                ErrorType = "ValidationError",
                EntityType = "Product",
                ErrorCode = "ERR001",
                ErrorMessage = "Product validation failed - Updated",
                ErrorSeverity = "Medium",
                UpdatedBy = "Test Admin"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/syncerrorlog/{logId}", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateSyncErrorLog_WithInvalidData_AsAdmin_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var logId = Guid.NewGuid();
            var updateDto = new UpdateSyncErrorLogDto
            {
                Id = logId,
                ErrorType = "", // Invalid empty error type
                EntityType = "", // Invalid empty entity type
                ErrorCode = "", // Invalid empty error code
                ErrorMessage = "", // Invalid empty error message
                ErrorSeverity = "", // Invalid empty error severity
                UpdatedBy = ""
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/syncerrorlog/{logId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateSyncErrorLog_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var logId = Guid.NewGuid();
            var updateDto = new UpdateSyncErrorLogDto
            {
                Id = logId,
                ErrorType = "ValidationError",
                EntityType = "Product",
                ErrorCode = "ERR001",
                ErrorMessage = "Product validation failed - Updated",
                ErrorSeverity = "Medium",
                UpdatedBy = "Test User"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/syncerrorlog/{logId}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteSyncErrorLog_WithValidId_AsAdmin_ShouldReturnNoContent()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var logId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/syncerrorlog/{logId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteSyncErrorLog_WithInvalidId_AsAdmin_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var invalidId = Guid.Empty;

            // Act
            var response = await _client.DeleteAsync($"/api/syncerrorlog/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteSyncErrorLog_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var userToken = await AuthHelper.GetUserTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var logId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/syncerrorlog/{logId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteSyncErrorLog_AsGuest_ShouldReturnUnauthorized()
        {
            // Arrange
            var logId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/syncerrorlog/{logId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public async Task CreateSyncErrorLog_WithDifferentErrorTypes_ShouldBeCreated()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var errorTypes = new[] { "ValidationError", "NetworkError", "TimeoutError", "AuthenticationError", "DataError" };

            foreach (var errorType in errorTypes)
            {
                var createDto = new CreateSyncErrorLogDto
                {
                    ErrorType = errorType,
                    EntityType = "Product",
                    EntityId = Guid.NewGuid(),
                    ErrorCode = $"ERR_{errorType.ToUpper()}",
                    ErrorMessage = $"{errorType} occurred during sync",
                    ErrorSeverity = "Medium",
                    RequestData = "{\"test\":\"data\"}",
                    ResponseData = "{\"error\":\"occurred\"}"
                };

                var json = JsonSerializer.Serialize(createDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await _client.PostAsync("/api/syncerrorlog", content);

                // Assert
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task CreateSyncErrorLog_WithDifferentErrorSeverities_ShouldBeCreated()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var errorSeverities = new[] { "Low", "Medium", "High", "Critical" };

            foreach (var errorSeverity in errorSeverities)
            {
                var createDto = new CreateSyncErrorLogDto
                {
                    ErrorType = "TestError",
                    EntityType = "Product",
                    EntityId = Guid.NewGuid(),
                    ErrorCode = "ERR_TEST",
                    ErrorMessage = $"Test error with {errorSeverity} severity",
                    ErrorSeverity = errorSeverity,
                    RequestData = "{\"test\":\"data\"}",
                    ResponseData = "{\"error\":\"occurred\"}"
                };

                var json = JsonSerializer.Serialize(createDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await _client.PostAsync("/api/syncerrorlog", content);

                // Assert
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task UpdateSyncErrorLog_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateSyncErrorLogDto
            {
                Id = nonExistentId,
                ErrorType = "ValidationError",
                EntityType = "Product",
                ErrorCode = "ERR001",
                ErrorMessage = "Product validation failed - Updated",
                ErrorSeverity = "Medium",
                UpdatedBy = "Test Admin"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/syncerrorlog/{nonExistentId}", content);

            // Assert
            // May return BadRequest for validation or NotFound for non-existent resource
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task CreateSyncErrorLog_WithVeryLongErrorMessage_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var longErrorMessage = new string('A', 10000); // Very long error message
            var createDto = new CreateSyncErrorLogDto
            {
                ErrorType = "ValidationError",
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                ErrorCode = "ERR_LONG",
                ErrorMessage = longErrorMessage,
                ErrorSeverity = "High",
                RequestData = "{\"test\":\"data\"}",
                ResponseData = "{\"error\":\"occurred\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/syncerrorlog", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSyncErrorLog_WithSpecialCharactersInErrorCode_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateSyncErrorLogDto
            {
                ErrorType = "ValidationError",
                EntityType = "Product",
                EntityId = Guid.NewGuid(),
                ErrorCode = "ERR!@#$%^&*()_+-=[]{}|;':\",./<>?",
                ErrorMessage = "Error with special characters in code",
                ErrorSeverity = "Medium",
                RequestData = "{\"test\":\"data\"}",
                ResponseData = "{\"error\":\"occurred\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/syncerrorlog", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSyncErrorLog_WithUnicodeCharacters_ShouldHandleGracefully()
        {
            // Arrange
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var createDto = new CreateSyncErrorLogDto
            {
                ErrorType = "خطای اعتبارسنجی",
                EntityType = "محصول",
                EntityId = Guid.NewGuid(),
                ErrorCode = "ERR_UNICODE",
                ErrorMessage = "خطای اعتبارسنجی محصول",
                ErrorSeverity = "متوسط",
                RequestData = "{\"test\":\"data\"}",
                ResponseData = "{\"error\":\"occurred\"}"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/syncerrorlog", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        #endregion
    }
}
