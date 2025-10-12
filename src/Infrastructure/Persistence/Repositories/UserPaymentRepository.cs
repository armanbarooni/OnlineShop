using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class UserPaymentRepository : IUserPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public UserPaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.UserPayments
                .AsNoTracking()
                .Include(up => up.User)
                .Include(up => up.Order)
                .FirstOrDefaultAsync(up => up.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<UserPayment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserPayments
                .AsNoTracking()
                .Include(up => up.User)
                .Include(up => up.Order)
                .Where(up => up.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            return await _context.UserPayments
                .AsNoTracking()
                .Include(up => up.User)
                .Include(up => up.Order)
                .Where(up => up.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserPayment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken)
        {
            return await _context.UserPayments
                .AsNoTracking()
                .Include(up => up.User)
                .Include(up => up.Order)
                .FirstOrDefaultAsync(up => up.TransactionId == transactionId, cancellationToken);
        }

        public async Task<IEnumerable<UserPayment>> GetByStatusAsync(string status, CancellationToken cancellationToken)
        {
            return await _context.UserPayments
                .AsNoTracking()
                .Include(up => up.User)
                .Include(up => up.Order)
                .Where(up => up.PaymentStatus == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserPayment>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.UserPayments
                .AsNoTracking()
                .Include(up => up.User)
                .Include(up => up.Order)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserPayment userPayment, CancellationToken cancellationToken)
        {
            await _context.UserPayments.AddAsync(userPayment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserPayment userPayment, CancellationToken cancellationToken)
        {
            _context.UserPayments.Update(userPayment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var userPayment = await _context.UserPayments.FindAsync(id, cancellationToken);
            if (userPayment != null)
            {
                userPayment.Delete(null);
                _context.UserPayments.Update(userPayment);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
