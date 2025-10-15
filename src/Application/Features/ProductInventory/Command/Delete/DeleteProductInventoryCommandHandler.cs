using MediatR;
using OnlineShop.Application.Common.Models;


using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.ProductInventory.Command.Delete
{
    public class DeleteProductInventoryCommandHandler : IRequestHandler<DeleteProductInventoryCommand, Result<bool>>
    {
        private readonly IProductInventoryRepository _repository;

        public DeleteProductInventoryCommandHandler(IProductInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteProductInventoryCommand request, CancellationToken cancellationToken)
        {
            var productInventory = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (productInventory == null)
                return Result<bool>.Failure("ProductInventory not found");

            await _repository.DeleteAsync(request.Id, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}


