using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Queries.GetRelatedProducts;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Product.Queries.GetRelatedProducts
{
    public class GetRelatedProductsQueryHandler : IRequestHandler<GetRelatedProductsQuery, Result<List<ProductDto>>>
    {
        private readonly IProductRelationRepository _productRelationRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetRelatedProductsQueryHandler(
            IProductRelationRepository productRelationRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _productRelationRepository = productRelationRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductDto>>> Handle(GetRelatedProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get related product IDs
                var relations = await _productRelationRepository.GetRelatedProductsAsync(
                    request.ProductId, 
                    request.RelationType, 
                    request.Limit, 
                    cancellationToken);

                if (!relations.Any())
                {
                    return Result<List<ProductDto>>.Success(new List<ProductDto>());
                }

                // Get related products with full details
                var relatedProductIds = relations.Select(r => r.RelatedProductId).ToList();
                var relatedProducts = await _productRepository.GetByIdsWithIncludesAsync(relatedProductIds, cancellationToken);

                // Map to DTOs
                var productDtos = _mapper.Map<List<ProductDto>>(relatedProducts);

                return Result<List<ProductDto>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<List<ProductDto>>.Failure($"خطا در دریافت محصولات مرتبط: {ex.Message}");
            }
        }
    }
}


