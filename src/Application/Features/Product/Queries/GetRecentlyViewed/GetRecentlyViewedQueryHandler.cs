using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Queries.GetRecentlyViewed;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Product.Queries.GetRecentlyViewed
{
    public class GetRecentlyViewedQueryHandler : IRequestHandler<GetRecentlyViewedQuery, Result<List<ProductDto>>>
    {
        private readonly IUserProductViewRepository _userProductViewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetRecentlyViewedQueryHandler(
            IUserProductViewRepository userProductViewRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _userProductViewRepository = userProductViewRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductDto>>> Handle(GetRecentlyViewedQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get recently viewed products
                var recentlyViewed = await _userProductViewRepository.GetRecentlyViewedAsync(
                    Guid.Parse(request.UserId), 
                    request.Limit, 
                    cancellationToken);

                if (!recentlyViewed.Any())
                {
                    return Result<List<ProductDto>>.Success(new List<ProductDto>());
                }

                // Get unique product IDs (remove duplicates)
                var productIds = recentlyViewed
                    .Select(rv => rv.ProductId)
                    .Distinct()
                    .ToList();

                // Get products with full details
                var products = await _productRepository.GetByIdsWithIncludesAsync(productIds, cancellationToken);

                // Map to DTOs
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                return Result<List<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<List<ProductDto>>.Failure($"خطا در دریافت محصولات اخیراً مشاهده شده: {ex.Message}");
            }
        }
    }
}


