namespace OnlineShop.Application.Contracts.Services
{
    public interface IPaymentGatewayService
    {
        Task<(bool Success, string Url, string Authority, string Message)> InitiatePaymentAsync(Guid orderId, long amount, string mobile, string description);
        Task<(bool Success, string RefId, string Message)> VerifyPaymentAsync(string authority, long amount);
    }
}
