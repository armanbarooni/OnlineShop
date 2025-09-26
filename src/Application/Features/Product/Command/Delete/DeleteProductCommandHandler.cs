using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Infrastructure.Persistence.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Product.Command.Delete
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<string>>
    {
        private readonly IProductRepository _repository;

        public DeleteProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<string>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
                return Result<string>.Failure("Product not found");

            product.Delete();
            await _repository.UpdateAsync(product);
            return Result<string>.Success("Product deleted successfully");
        }
    }
}
