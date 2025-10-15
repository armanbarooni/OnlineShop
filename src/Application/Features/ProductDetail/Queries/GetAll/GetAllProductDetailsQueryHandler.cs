using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.ProductDetail;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductDetail.Queries.GetAll
{
    public class GetAllProductDetailsQueryHandler : IRequestHandler<GetAllProductDetailsQuery, Result<IEnumerable<ProductDetailDto>>>
    {
        private readonly IProductDetailRepository _repository;
        private readonly IMapper _mapper;

        public GetAllProductDetailsQueryHandler(IProductDetailRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProductDetailDto>>> Handle(GetAllProductDetailsQuery request, CancellationToken cancellationToken)
        {
            var productDetails = await _repository.GetAllAsync(cancellationToken);
            var productDetailDtos = _mapper.Map<IEnumerable<ProductDetailDto>>(productDetails);
            return Result<IEnumerable<ProductDetailDto>>.Success(productDetailDtos);
        }
    }
}

