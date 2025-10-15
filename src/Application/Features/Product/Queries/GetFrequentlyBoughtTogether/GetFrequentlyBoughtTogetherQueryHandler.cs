using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Queries.GetFrequentlyBoughtTogether;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Product.Queries.GetFrequentlyBoughtTogether
{
    public class GetFrequentlyBoughtTogetherQueryHandler : IRequestHandler<GetFrequentlyBoughtTogetherQuery, Result<List<ProductDto>>>
    {
        private readonly IUserProductViewRepository _userProductViewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetFrequentlyBoughtTogetherQueryHandler(
            IUserProductViewRepository userProductViewRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _userProductViewRepository = userProductViewRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductDto>>> Handle(GetFrequentlyBoughtTogetherQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get frequently bought together product IDs
                var frequentlyBoughtProductIds = await _userProductViewRepository.GetFrequentlyBoughtTogetherAsync(
                    request.ProductId, 
                    request.Limit, 
                    cancellationToken);

                if (!frequentlyBoughtProductIds.Any())
                {
                    return Result<List<ProductDto>>.Success(new List<ProductDto>());
                }

                // Get products with full details
                var products = await _productRepository.GetByIdsWithIncludesAsync(frequentlyBoughtProductIds, cancellationToken);

                // Map to DTOs
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                return Result<List<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<List<ProductDto>>.Failure($"خطا در دریافت محصولات مرتبط: {ex.Message}");
            }
        }
    }
}


