using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Infrastructure.Persistence.Repositories;
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
            var product = await _repository.GetByIdAsync(request.Product.Id);
            if (product == null)
                return Result<ProductDto>.Failure("Product not found");

            product.Update(request.Product.Name, request.Product.Description, request.Product.Price, request.Product.StockQuantity);
            await _repository.UpdateAsync(product);
            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
        }
    }
}
