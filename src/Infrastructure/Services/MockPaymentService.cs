using OnlineShop.Application.Contracts.Services;

namespace OnlineShop.Infrastructure.Services
{
    public class MockPaymentService : IPaymentGatewayService
    {
        public Task<(bool Success, string Url, string Authority, string Message)> InitiatePaymentAsync(Guid orderId, long amount, string mobile, string description)
        {
            // Simulate success
            var authority = Guid.NewGuid().ToString();
            var url = $"http://localhost:5000/api/Payment/mock-gateway?authority={authority}&amount={amount}&status=OK"; 
            // In real scenario, URL points to bank. Here we point to a mock endpoint or just return success.
            
            return Task.FromResult((true, url, authority, "Successfully initiated"));
        }

        public Task<(bool Success, string RefId, string Message)> VerifyPaymentAsync(string authority, long amount)
        {
            // Simulate verification
            return Task.FromResult((true, new Random().Next(100000, 999999).ToString(), "پرداخت با موفقیت انجام شد"));
        }
    }
}
