using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Exceptions;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.Product.Queries.GetById
{
    public class GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper) : IRequestHandler<GetProductByIdQuery, Result<ProductDetailsDto>>
    {
        public async Task<Result<ProductDetailsDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDetailsDto>.Failure($"Product with ID {request.Id} not found");
            }

            var dto = mapper.Map<ProductDetailsDto>(product);
            return Result<ProductDetailsDto>.Success(dto);
        }

    }
}