using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductInventory.Command.BulkUpdate
{
    public class BulkUpdateProductInventoryCommandHandler : IRequestHandler<BulkUpdateProductInventoryCommand, Result<bool>>
    {
        private readonly IProductInventoryRepository _repository;
        private readonly IProductRepository _productRepository;

        public BulkUpdateProductInventoryCommandHandler(
            IProductInventoryRepository repository,
            IProductRepository productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
        }

        public async Task<Result<bool>> Handle(BulkUpdateProductInventoryCommand request, CancellationToken cancellationToken)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return Result<bool>.Failure("No items provided for bulk update");
            }

            var updatedCount = 0;

            foreach (var item in request.Items)
            {
                // Check if product exists
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null)
                {
                    continue; // Skip non-existent products
                }

                // Get or create inventory
                var inventory = await _repository.GetByProductIdAsync(item.ProductId, cancellationToken);
                
                if (inventory == null)
                {
                    // Create new inventory if doesn't exist
                    inventory = Domain.Entities.ProductInventory.Create(
                        item.ProductId,
                        item.Quantity,
                        0, // ReservedQuantity
                        0  // SoldQuantity
                    );
                    await _repository.AddAsync(inventory, cancellationToken);
                }
                else
                {
                    // Update existing inventory
                    inventory.SetAvailableQuantity(item.Quantity);
                    await _repository.UpdateAsync(inventory, cancellationToken);
                }

                updatedCount++;
            }

            if (updatedCount == 0)
            {
                return Result<bool>.Failure("No inventories were updated");
            }

            return Result<bool>.Success(true);
        }
    }
}

