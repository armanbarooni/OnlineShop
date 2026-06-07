namespace OnlineShop.Application.Contracts.Services
{
    public interface IMahakOrderSyncService
    {
        Task SyncOrderToMahakAsync(Guid orderId, CancellationToken cancellationToken);
    }
}
