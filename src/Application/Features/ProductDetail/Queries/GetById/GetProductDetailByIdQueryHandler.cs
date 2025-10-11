using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.ProductDetail;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.ProductDetail.Queries.GetById
{
    public class GetProductDetailByIdQueryHandler : IRequestHandler<GetProductDetailByIdQuery, Result<ProductDetailDto>>
    {
        private readonly IProductDetailRepository _repository;
        private readonly IMapper _mapper;

        public GetProductDetailByIdQueryHandler(IProductDetailRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductDetailDto>> Handle(GetProductDetailByIdQuery request, CancellationToken cancellationToken)
        {
            var productDetail = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (productDetail == null)
                return Result<ProductDetailDto>.Failure("ProductDetail not found");

            return Result<ProductDetailDto>.Success(_mapper.Map<ProductDetailDto>(productDetail));
        }
    }
}
