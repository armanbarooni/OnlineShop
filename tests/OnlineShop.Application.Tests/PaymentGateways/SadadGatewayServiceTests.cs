using Moq;
using Moq.Protected;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using OnlineShop.Infrastructure.PaymentGateways.Sadad;
using OnlineShop.Infrastructure.PaymentGateways.Sadad.Configuration;
using OnlineShop.Infrastructure.PaymentGateways.Sadad.Models;
using OnlineShop.Domain.Interfaces.Services;

namespace OnlineShop.Application.Tests.PaymentGateways
{
    public class SadadGatewayServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<SadadGatewayService>> _loggerMock;
        private readonly SadadGatewayConfig _config;
        private readonly SadadGatewayService _service;

        public SadadGatewayServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<SadadGatewayService>>();
            
            _config = new SadadGatewayConfig
            {
                MerchantId = "test-merchant",
                TerminalId = "test-terminal",
                Key = "test-key",
                BaseUrl = "https://sadad.shaparak.ir/api/v0",
                SandboxUrl = "https://sandbox.sadad.shaparak.ir/api/v0",
                UseSandbox = true,
                CallbackUrl = "https://test.com/callback"
            };

            var configOptions = Options.Create(_config);
            _service = new SadadGatewayService(configOptions, _httpClientFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task RequestPaymentAsync_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var paymentRequest = new PaymentRequest
            {
                PaymentId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Amount = 100000m,
                CallbackUrl = "https://test.com/callback"
            };

            var mockResponse = new SadadPaymentResponse
            {
                ResCode = 0,
                Token = "test-token-123",
                Description = "Success"
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
            _httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClient);

            // Act
            var result = await _service.RequestPaymentAsync(paymentRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.PaymentUrl);
            Assert.Equal("test-token-123", result.Token);
        }

        [Fact]
        public async Task RequestPaymentAsync_WithInvalidResponse_ShouldReturnFailure()
        {
            // Arrange
            var paymentRequest = new PaymentRequest
            {
                PaymentId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Amount = 100000m,
                CallbackUrl = "https://test.com/callback"
            };

            var mockResponse = new SadadPaymentResponse
            {
                ResCode = 1,
                Token = null,
                Description = "Invalid Merchant"
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
            _httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClient);

            // Act
            var result = await _service.RequestPaymentAsync(paymentRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Token);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task VerifyPaymentAsync_WithValidToken_ShouldReturnSuccess()
        {
            // Arrange
            var token = "test-token-123";
            var amount = 100000m;

            var mockResponse = new SadadVerifyResponse
            {
                ResCode = 0,
                Amount = 1000000, // Amount in Rials
                RetrivalRefNo = "retrieval-ref-123",
                SystemTraceNo = "system-trace-456",
                OrderId = Guid.NewGuid().ToString()
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
            _httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClient);

            // Act
            var result = await _service.VerifyPaymentAsync(token, amount);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsVerified);
            Assert.NotNull(result.RetrievalRefNo);
        }

        [Fact]
        public async Task VerifyPaymentAsync_WithAmountMismatch_ShouldReturnFailure()
        {
            // Arrange
            var token = "test-token-123";
            var amount = 100000m;

            var mockResponse = new SadadVerifyResponse
            {
                ResCode = 0,
                Amount = 2000000, // Different amount
                RetrivalRefNo = "retrieval-ref-123",
                SystemTraceNo = "system-trace-456"
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
            _httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClient);

            // Act
            var result = await _service.VerifyPaymentAsync(token, amount);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsVerified);
            Assert.Contains("مبلغ", result.ErrorMessage ?? "");
        }

        [Fact]
        public async Task RefundPaymentAsync_WithValidTransaction_ShouldReturnSuccess()
        {
            // Arrange
            var transactionId = "retrieval-ref-123";
            var amount = 100000m;

            var mockResponse = new SadadRefundResponse
            {
                ResCode = 0,
                RefundId = "refund-id-123",
                Description = "Success"
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
            _httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClient);

            // Act
            var result = await _service.RefundPaymentAsync(transactionId, amount);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.RefundId);
        }

        private HttpClient CreateMockHttpClient<T>(HttpStatusCode statusCode, T responseObject)
        {
            var jsonContent = JsonSerializer.Serialize(responseObject);
            var httpResponse = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            var handler = new Mock<HttpMessageHandler>();
            
            // Fix: Use Protected() to mock the protected SendAsync method
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            return new HttpClient(handler.Object)
            {
                BaseAddress = new Uri(_config.GetBaseUrl())
            };
        }
    }
}

