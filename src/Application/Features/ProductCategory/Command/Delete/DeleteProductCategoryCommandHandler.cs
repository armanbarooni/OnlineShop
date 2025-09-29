using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Exceptions;

namespace OnlineShop.Application.Features.ProductCategory.Command.Delete
{
    public class DeleteProductCategoryCommandHandler
    {
        private readonly IProductCategoryRepository _repository;

        public DeleteProductCategoryCommandHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteProductCategoryCommand command, CancellationToken cancellationToken)
        {
            var productCategory = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (productCategory == null)
                throw new NotFoundException($"ProductCategory with ID {command.Id} not found.");

            productCategory.Delete(null);
            await _repository.UpdateAsync(productCategory, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
