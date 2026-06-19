using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Infrastructure.Services
{
    public interface IMahakTrafficLogger
    {
        Task LogRequestAsync(
            string operation,
            string endpoint,
            string comment,
            object? payload,
            CancellationToken cancellationToken);

        Task LogResponseAsync(
            string operation,
            string endpoint,
            string comment,
            int statusCode,
            bool isSuccessStatusCode,
            string? payload,
            CancellationToken cancellationToken);
    }
}
