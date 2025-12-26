using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.DTOs.ProductComparison;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.ProductComparison.Queries.CompareProducts
{
    public class CompareProductsQueryHandler : IRequestHandler<CompareProductsQuery, Result<ComparisonResultDto>>
    {
        private readonly IProductComparisonRepository _comparisonRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CompareProductsQueryHandler(
            IProductComparisonRepository comparisonRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _comparisonRepository = comparisonRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<ComparisonResultDto>> Handle(CompareProductsQuery request, CancellationToken cancellationToken)
        {
            var comparison = await _comparisonRepository.GetByUserIdAsync(Guid.Parse(request.UserId), cancellationToken);
            if (comparison == null || !comparison.ProductIds.Any())
                return Result<ComparisonResultDto>.Success(new ComparisonResultDto());

            // Fetch products with all details
            var allProducts = await _productRepository.GetAllWithIncludesAsync(cancellationToken);
            var products = allProducts.Where(p => comparison.ProductIds.Contains(p.Id)).ToList();

            var productDtos = _mapper.Map<List<ProductDto>>(products);

            var facets = new ComparisonFacetsDto
            {
                Brands = products.Where(p => p.Brand != null).Select(p => p.Brand!.Name).Distinct().ToList(),
                Prices = products.Select(p => p.Price).Distinct().ToList(),
                Sizes = products.SelectMany(p => p.ProductVariants).Select(v => v.Size ?? "").Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList(),
                Colors = products.SelectMany(p => p.ProductVariants).Select(v => v.Color ?? "").Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList(),
                Materials = products.SelectMany(p => p.ProductMaterials).Select(pm => pm.Material.Name).Distinct().ToList()
            };

            var result = new ComparisonResultDto
            {
                Products = productDtos,
                Facets = facets
            };

            return Result<ComparisonResultDto>.Success(result);
        }
    }
}

