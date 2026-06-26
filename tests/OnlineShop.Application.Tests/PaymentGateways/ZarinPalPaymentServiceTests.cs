using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OnlineShop.Infrastructure.Services;
using Xunit;

namespace OnlineShop.Application.Tests.PaymentGateways
{
    public class ZarinPalPaymentServiceTests
    {
        [Fact]
        public async Task InitiatePaymentAsync_ShouldSendAmountInToman()
        {
            var (service, handler) = CreateService();

            handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"data":{"code":100,"authority":"AUTH-123"}}""", Encoding.UTF8, "application/json")
            };

            var result = await service.InitiatePaymentAsync(Guid.NewGuid(), 10000, "09120000000", "test order");

            Assert.True(result.Success);
            Assert.Equal(1000, handler.RequestAmount);
        }

        [Fact]
        public async Task VerifyPaymentAsync_ShouldSendSameConvertedAmount()
        {
            var (service, handler) = CreateService();

            handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"data":{"code":100,"ref_id":123456}}""", Encoding.UTF8, "application/json")
            };

            var result = await service.VerifyPaymentAsync("AUTH-123", 25000);

            Assert.True(result.Success);
            Assert.Equal(2500, handler.RequestAmount);
        }

        private static (ZarinPalPaymentService Service, CapturingHandler Handler) CreateService()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ZarinPal:MerchantId"] = "test-merchant",
                    ["ZarinPal:CallbackUrl"] = "https://example.com/callback"
                })
                .Build();

            var handler = new CapturingHandler();
            var httpClient = new HttpClient(handler);
            var logger = new LoggerFactory().CreateLogger<ZarinPalPaymentService>();
            var service = new ZarinPalPaymentService(configuration, logger, httpClient);

            return (service, handler);
        }

        private sealed class CapturingHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }

            public long RequestAmount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.Content != null)
                {
                    var body = request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
                    using var document = JsonDocument.Parse(body);
                    RequestAmount = document.RootElement.GetProperty("amount").GetInt64();
                }

                var response = ResponseFactory?.Invoke(request) ?? new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"data":{"code":100,"authority":"AUTH-123"}}""", Encoding.UTF8, "application/json")
                };

                return Task.FromResult(response);
            }
        }
    }
}
