using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Application.DTOs.Common;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Queries.Search
{
    public class ProductSearchQueryHandler : IRequestHandler<ProductSearchQuery, Result<ProductSearchResultDto>>
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public ProductSearchQueryHandler(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProductSearchResultDto>> Handle(ProductSearchQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var criteria = request.Criteria ?? new ProductSearchCriteriaDto();

                // Get products with all includes for filtering
                var allProducts = await _repository.GetAllWithIncludesAsync(cancellationToken);
                var query = allProducts.AsQueryable();

                // Apply filters
                query = ApplyFilters(query, criteria);

                // Apply sorting
                query = ApplySorting(query, criteria);

                // Get total count
                var totalCount = query.Count();

                // Apply pagination
                var products = query
                    .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .ToList();

                var productDtos = _mapper.Map<List<ProductDto>>(products);

                var pagedResult = PagedResultDto<ProductDto>.Create(
                    productDtos,
                    totalCount,
                    criteria.PageNumber,
                    criteria.PageSize
                );

                // Generate facets
                var facets = await GenerateFacets(allProducts, criteria, cancellationToken);

                var searchResult = new ProductSearchResultDto
                {
                    Products = pagedResult,
                    AvailableSizes = facets.AvailableSizes,
                    AvailableColors = facets.AvailableColors,
                    AvailableBrands = facets.AvailableBrands,
                    AvailableMaterials = facets.AvailableMaterials,
                    AvailableSeasons = facets.AvailableSeasons,
                    PriceRanges = facets.PriceRanges
                };

                return Result<ProductSearchResultDto>.Success(searchResult);
            }
            catch (Exception ex)
            {
                return Result<ProductSearchResultDto>.Failure($"خطا در جستجوی محصولات: {ex.Message}");
            }
        }

        private IQueryable<Domain.Entities.Product> ApplyFilters(IQueryable<Domain.Entities.Product> query, ProductSearchCriteriaDto criteria)
        {
            // Search by term (Name, Description, SKU, Barcode, Brand.Name, Material.Name)
            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTerm = criteria.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                    (p.Sku != null && p.Sku.ToLower().Contains(searchTerm)) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm)) ||
                    (p.Brand != null && p.Brand.Name.ToLower().Contains(searchTerm)) ||
                    p.ProductMaterials.Any(pm => pm.Material.Name.ToLower().Contains(searchTerm)));
            }

            // Filter by category
            if (criteria.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);
            }

            // Filter by brand
            if (criteria.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == criteria.BrandId.Value);
            }

            // Filter by gender
            if (!string.IsNullOrWhiteSpace(criteria.Gender))
            {
                query = query.Where(p => p.Gender == criteria.Gender);
            }

            // Filter by sizes (in ProductVariants)
            if (criteria.Sizes != null && criteria.Sizes.Any())
            {
                query = query.Where(p => p.ProductVariants.Any(v => criteria.Sizes.Contains(v.Size)));
            }

            // Filter by colors (in ProductVariants)
            if (criteria.Colors != null && criteria.Colors.Any())
            {
                query = query.Where(p => p.ProductVariants.Any(v => criteria.Colors.Contains(v.Color)));
            }

            // Filter by materials
            if (criteria.MaterialIds != null && criteria.MaterialIds.Any())
            {
                query = query.Where(p => p.ProductMaterials.Any(pm => criteria.MaterialIds.Contains(pm.MaterialId)));
            }

            // Filter by seasons
            if (criteria.SeasonIds != null && criteria.SeasonIds.Any())
            {
                query = query.Where(p => p.ProductSeasons.Any(ps => criteria.SeasonIds.Contains(ps.SeasonId)));
            }

            // Filter by new arrivals (last 30 days)
            if (criteria.NewArrivals.HasValue && criteria.NewArrivals.Value)
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                query = query.Where(p => p.CreatedAt >= thirtyDaysAgo);
            }

            // Filter by on sale
            if (criteria.OnSale.HasValue && criteria.OnSale.Value)
            {
                var now = DateTime.UtcNow;
                query = query.Where(p => p.SalePrice.HasValue && 
                    (!p.SaleStartDate.HasValue || p.SaleStartDate.Value <= now) &&
                    (!p.SaleEndDate.HasValue || p.SaleEndDate.Value >= now));
            }

            // Filter by price range
            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= criteria.MinPrice.Value);
            }

            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= criteria.MaxPrice.Value);
            }

            // Filter by active status
            if (criteria.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == criteria.IsActive.Value);
            }

            // Filter by featured status
            if (criteria.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == criteria.IsFeatured.Value);
            }

            // Filter by stock availability
            if (criteria.InStock.HasValue && criteria.InStock.Value)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            return query;
        }

        private IQueryable<Domain.Entities.Product> ApplySorting(IQueryable<Domain.Entities.Product> query, ProductSearchCriteriaDto criteria)
        {
            return criteria.SortBy?.ToLower() switch
            {
                "name" => criteria.SortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "price" => criteria.SortDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "viewcount" => criteria.SortDescending
                    ? query.OrderByDescending(p => p.ViewCount)
                    : query.OrderBy(p => p.ViewCount),
                "createdat" => criteria.SortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt) // Default sort by newest
            };
        }

        private async Task<FacetData> GenerateFacets(IEnumerable<Domain.Entities.Product> allProducts, ProductSearchCriteriaDto criteria, CancellationToken cancellationToken)
        {
            var products = allProducts.ToList();

            var facets = new FacetData
            {
                AvailableSizes = products
                    .SelectMany(p => p.ProductVariants)
                    .Select(v => v.Size)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList(),

                AvailableColors = products
                    .SelectMany(p => p.ProductVariants)
                    .Select(v => v.Color)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList(),

                AvailableBrands = products
                    .Where(p => p.Brand != null)
                    .GroupBy(p => new { p.Brand!.Id, p.Brand.Name })
                    .Select(g => new BrandFacetDto
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        Count = g.Count()
                    })
                    .OrderBy(b => b.Name)
                    .ToList(),

                AvailableMaterials = products
                    .SelectMany(p => p.ProductMaterials)
                    .GroupBy(pm => new { pm.Material.Id, pm.Material.Name })
                    .Select(g => new MaterialFacetDto
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        Count = g.Count()
                    })
                    .OrderBy(m => m.Name)
                    .ToList(),

                AvailableSeasons = products
                    .SelectMany(p => p.ProductSeasons)
                    .GroupBy(ps => new { ps.Season.Id, ps.Season.Name })
                    .Select(g => new SeasonFacetDto
                    {
                        Id = g.Key.Id,
                        Name = g.Key.Name,
                        Count = g.Count()
                    })
                    .OrderBy(s => s.Name)
                    .ToList()
            };

            // Generate price ranges
            if (products.Any())
            {
                var minPrice = products.Min(p => p.Price);
                var maxPrice = products.Max(p => p.Price);
                var priceRange = (maxPrice - minPrice) / 5; // 5 price ranges

                facets.PriceRanges = new List<PriceRangeDto>();
                for (int i = 0; i < 5; i++)
                {
                    var rangeMin = minPrice + (i * priceRange);
                    var rangeMax = i == 4 ? maxPrice : minPrice + ((i + 1) * priceRange);
                    var count = products.Count(p => p.Price >= rangeMin && p.Price <= rangeMax);

                    facets.PriceRanges.Add(new PriceRangeDto
                    {
                        MinPrice = Math.Round(rangeMin, 2),
                        MaxPrice = Math.Round(rangeMax, 2),
                        Count = count
                    });
                }
            }

            return facets;
        }

        private class FacetData
        {
            public List<string> AvailableSizes { get; set; } = new();
            public List<string> AvailableColors { get; set; } = new();
            public List<BrandFacetDto> AvailableBrands { get; set; } = new();
            public List<MaterialFacetDto> AvailableMaterials { get; set; } = new();
            public List<SeasonFacetDto> AvailableSeasons { get; set; } = new();
            public List<PriceRangeDto> PriceRanges { get; set; } = new();
        }
    }
}


