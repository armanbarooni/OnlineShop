using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Features.ProductImage.Queries.GetAll
{
    public class GetAllProductImagesQueryHandler : IRequestHandler<GetAllProductImagesQuery, Result<IEnumerable<ProductImageDto>>>
    {
        private readonly IProductImageRepository _repository;
        private readonly IMapper _mapper;

        public GetAllProductImagesQueryHandler(IProductImageRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProductImageDto>>> Handle(GetAllProductImagesQuery request, CancellationToken cancellationToken)
        {
            var productImages = await _repository.GetAllAsync(cancellationToken);
            var productImageDtos = _mapper.Map<IEnumerable<ProductImageDto>>(productImages);
            return Result<IEnumerable<ProductImageDto>>.Success(productImageDtos);
        }
    }
}
