using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

namespace OnlineShop.Application.Features.ProductCategory.Command.Delete
{
    public class DeleteProductCategoryCommandHandler
        : IRequestHandler<DeleteProductCategoryCommand, Result<bool>>
    {
        private readonly IProductCategoryRepository _repository;

        public DeleteProductCategoryCommandHandler(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(
            DeleteProductCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var productCategory = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (productCategory == null)
                return Result<bool>.Failure($"ProductCategory with Id {request.Id} not found");

            await _repository.DeleteAsync(productCategory, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
