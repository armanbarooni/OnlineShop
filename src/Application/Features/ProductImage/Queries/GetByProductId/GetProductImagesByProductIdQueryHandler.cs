using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductImage;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductImage.Queries.GetByProductId
{
    public class GetProductImagesByProductIdQueryHandler : IRequestHandler<GetProductImagesByProductIdQuery, Result<IEnumerable<ProductImageDto>>>
    {
        private readonly IProductImageRepository _repository;
        private readonly IMapper _mapper;

        public GetProductImagesByProductIdQueryHandler(IProductImageRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProductImageDto>>> Handle(GetProductImagesByProductIdQuery request, CancellationToken cancellationToken)
        {
            var productImages = await _repository.GetByProductIdAsync(request.ProductId, cancellationToken);
            var productImageDtos = _mapper.Map<IEnumerable<ProductImageDto>>(productImages);
            return Result<IEnumerable<ProductImageDto>>.Success(productImageDtos);
        }
    }
}

