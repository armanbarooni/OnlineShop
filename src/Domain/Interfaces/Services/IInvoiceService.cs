namespace OnlineShop.Domain.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoicePdfAsync(Guid orderId, CancellationToken cancellationToken);
    }
}

