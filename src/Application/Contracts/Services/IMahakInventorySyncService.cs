namespace OnlineShop.Application.Contracts.Services
{
    public interface IMahakInventorySyncService
    {
        Task SyncInventoryFromMahakAsync(CancellationToken cancellationToken);
    }
}
