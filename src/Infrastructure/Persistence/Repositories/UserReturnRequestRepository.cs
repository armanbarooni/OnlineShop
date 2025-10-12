using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class UserReturnRequestRepository : IUserReturnRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public UserReturnRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserReturnRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.UserReturnRequests
                .AsNoTracking()
                .Include(urr => urr.User)
                .Include(urr => urr.Order)
                .FirstOrDefaultAsync(urr => urr.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<UserReturnRequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserReturnRequests
                .AsNoTracking()
                .Include(urr => urr.User)
                .Include(urr => urr.Order)
                .Where(urr => urr.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserReturnRequest>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            return await _context.UserReturnRequests
                .AsNoTracking()
                .Include(urr => urr.User)
                .Include(urr => urr.Order)
                .Where(urr => urr.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserReturnRequest>> GetByStatusAsync(string status, CancellationToken cancellationToken)
        {
            return await _context.UserReturnRequests
                .AsNoTracking()
                .Include(urr => urr.User)
                .Include(urr => urr.Order)
                .Where(urr => urr.ReturnStatus == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserReturnRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken)
        {
            return await _context.UserReturnRequests
                .AsNoTracking()
                .Include(urr => urr.User)
                .Include(urr => urr.Order)
                .Where(urr => urr.ReturnStatus == "Pending")
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserReturnRequest>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.UserReturnRequests
                .AsNoTracking()
                .Include(urr => urr.User)
                .Include(urr => urr.Order)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserReturnRequest userReturnRequest, CancellationToken cancellationToken)
        {
            await _context.UserReturnRequests.AddAsync(userReturnRequest, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserReturnRequest userReturnRequest, CancellationToken cancellationToken)
        {
            _context.UserReturnRequests.Update(userReturnRequest);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var userReturnRequest = await _context.UserReturnRequests.FindAsync(id, cancellationToken);
            if (userReturnRequest != null)
            {
                userReturnRequest.Delete(null);
                _context.UserReturnRequests.Update(userReturnRequest);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
