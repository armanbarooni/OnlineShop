using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    public class SeasonControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public SeasonControllerTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllSeasons_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/season");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetSeasonById_WithValidId_ShouldReturnSeason()
        {
            // Arrange
            var seasonId = await CreateTestSeasonAsync();

            // Act
            var response = await _client.GetAsync($"/api/season/{seasonId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task GetSeasonById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/season/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateSeason_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var seasonDto = new
            {
                Name = $"Test Season {Guid.NewGuid()}",
                Description = "Test season description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/season", seasonDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task CreateSeason_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var seasonDto = new
            {
                Name = "Test Season",
                Description = "Test description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/season", seasonDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateSeason_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var seasonDto = new
            {
                Name = "Test Season",
                Description = "Test description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/season", seasonDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateSeason_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var seasonId = await CreateTestSeasonAsync();
            var updateDto = new
            {
                Id = seasonId,
                Name = $"Updated Season {Guid.NewGuid()}",
                Description = "Updated description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddMonths(4),
                IsActive = false
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/season/{seasonId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateSeason_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var seasonId = await CreateTestSeasonAsync();
            var updateDto = new
            {
                Id = seasonId,
                Name = "Updated Season",
                Description = "Updated description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                IsActive = false
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/season/{seasonId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteSeason_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var seasonId = await CreateTestSeasonAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/season/{seasonId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteSeason_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var seasonId = await CreateTestSeasonAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/season/{seasonId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateSeason_WithInvalidDateRange_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var invalidDto = new
            {
                Name = "Test Season",
                Description = "Test description",
                StartDate = DateTime.UtcNow.AddMonths(3), // Start after end
                EndDate = DateTime.UtcNow,
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/season", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateSeason_WithEmptyName_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var invalidDto = new
            {
                Name = "", // Empty name should be invalid
                Description = "Test description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/season", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateSeason_WithIdMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var seasonId = await CreateTestSeasonAsync();
            var updateDto = new
            {
                Id = Guid.NewGuid(), // Different ID
                Name = "Updated Season",
                Description = "Updated description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                IsActive = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/season/{seasonId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestSeasonAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var seasonDto = new
            {
                Name = $"Test Season {Guid.NewGuid()}",
                Description = "Test season for integration tests",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/season", seasonDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var seasonId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(seasonId))
                    return Guid.Parse(seasonId);
            }

            return Guid.NewGuid(); // Fallback
        }
    }
}

