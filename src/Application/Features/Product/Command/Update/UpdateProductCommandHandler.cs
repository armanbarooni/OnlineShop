using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Domain.Interfaces.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Product.Command.Update
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Product?.Id ?? Guid.Empty, cancellationToken);
            if (product == null)
                return Result<ProductDto>.Failure("Product not found");

            product.Update(
                request.Product?.Name ?? string.Empty, 
                request.Product?.Description ?? string.Empty, 
                request.Product?.Price ?? 0, 
                request.Product?.StockQuantity ?? 0, 
                null);
            
            // Update CategoryId if provided
            if (request.Product?.CategoryId.HasValue == true)
            {
                product.SetCategoryId(request.Product.CategoryId.Value);
            }
            else if (request.Product?.CategoryId == null && request.Product != null)
            {
                // If CategoryId is explicitly set to null, clear it
                product.SetCategoryId(null);
            }
            
            await _repository.UpdateAsync(product,cancellationToken);
            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
        }
    }
}
