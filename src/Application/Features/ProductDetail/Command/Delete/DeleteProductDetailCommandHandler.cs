using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductDetail.Command.Delete
{
    public class DeleteProductDetailCommandHandler : IRequestHandler<DeleteProductDetailCommand, Result<bool>>
    {
        private readonly IProductDetailRepository _repository;

        public DeleteProductDetailCommandHandler(IProductDetailRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteProductDetailCommand request, CancellationToken cancellationToken)
        {
            var productDetail = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (productDetail == null)
                return Result<bool>.Failure("ProductDetail not found");

            await _repository.DeleteAsync(request.Id, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}

