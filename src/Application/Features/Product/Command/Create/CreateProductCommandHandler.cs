using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Product.Command.Create
{
    public class CreateProductCommandHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {

            var product = Domain.Entities.Product.Create(
                request.Product.Name,
                request.Product.Description,
                request.Product.Price,
                request.Product.StockQuantity            
            );

            await repository.AddAsync(product, cancellationToken);
            return Result<ProductDto>.Success(mapper.Map<ProductDto>(product));
        }
    }
}