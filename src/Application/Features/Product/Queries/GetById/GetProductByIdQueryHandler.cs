using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Exceptions;

namespace OnlineShop.Application.Features.Product.Queries.GetById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDetailsDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<ProductDetailsDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                throw new NotFoundException(nameof(Product), request.Id);

            var dto = _mapper.Map<ProductDetailsDto>(product);
            return Result<ProductDetailsDto>.Success(dto);
        }
    }
}
