using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductImage.Command.Delete
{
    public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, Result<bool>>
    {
        private readonly IProductImageRepository _repository;

        public DeleteProductImageCommandHandler(IProductImageRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
        {
            var productImage = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (productImage == null)
                return Result<bool>.Failure("ProductImage not found");

            await _repository.DeleteAsync(request.Id, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}

