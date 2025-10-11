using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class UserOrderItemRepository : IUserOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public UserOrderItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserOrderItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.UserOrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(uoi => uoi.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<UserOrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            return await _context.UserOrderItems
                .AsNoTracking()
                .Where(uoi => uoi.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserOrderItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.UserOrderItems
                .AsNoTracking()
                .Where(uoi => uoi.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserOrderItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.UserOrderItems
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserOrderItem userOrderItem, CancellationToken cancellationToken)
        {
            await _context.UserOrderItems.AddAsync(userOrderItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserOrderItem userOrderItem, CancellationToken cancellationToken)
        {
            _context.UserOrderItems.Update(userOrderItem);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var userOrderItem = await _context.UserOrderItems.FindAsync(id, cancellationToken);
            if (userOrderItem != null)
            {
                userOrderItem.Delete(null);
                _context.UserOrderItems.Update(userOrderItem);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var orderItems = await _context.UserOrderItems
                .Where(uoi => uoi.OrderId == orderId)
                .ToListAsync(cancellationToken);

            foreach (var item in orderItems)
            {
                item.Delete(null);
            }

            if (orderItems.Any())
            {
                _context.UserOrderItems.UpdateRange(orderItems);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
