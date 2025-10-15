using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Common;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class OrderStatusHistoryRepository : GenericRepository<OrderStatusHistory>, IOrderStatusHistoryRepository
    {
        public OrderStatusHistoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.OrderStatusHistories
                .Where(osh => osh.OrderId == orderId && !osh.Deleted)
                .OrderBy(osh => osh.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<OrderStatusHistory>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.OrderStatusHistories
                .Where(osh => osh.OrderId == orderId && !osh.Deleted)
                .OrderBy(osh => osh.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<OrderStatusHistory?> GetLatestByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.OrderStatusHistories
                .Where(osh => osh.OrderId == orderId && !osh.Deleted)
                .OrderByDescending(osh => osh.ChangedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<OrderStatusHistory>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            return await _context.OrderStatusHistories
                .Where(osh => osh.Status.ToString() == status && !osh.Deleted)
                .OrderBy(osh => osh.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<OrderStatusHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.OrderStatusHistories
                .Where(osh => osh.ChangedAt >= startDate && osh.ChangedAt <= endDate && !osh.Deleted)
                .OrderBy(osh => osh.ChangedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
