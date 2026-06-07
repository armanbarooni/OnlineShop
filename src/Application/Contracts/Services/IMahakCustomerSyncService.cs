namespace OnlineShop.Application.Contracts.Services
{
    public interface IMahakCustomerSyncService
    {
        Task SyncCustomerToMahakAsync(Guid userId, CancellationToken cancellationToken);
    }
}
