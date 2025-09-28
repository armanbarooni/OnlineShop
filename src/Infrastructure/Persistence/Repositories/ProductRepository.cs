using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }


        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Products.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Products.AnyAsync(p => p.Name == name, cancellationToken);
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(id, cancellationToken);
            if (product != null)
            {
                product.Delete(1);
                _context.Products.Update(product);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}