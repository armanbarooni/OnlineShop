using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Common;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Application.Features.Product.Queries.Search
{
    public class ProductSearchQueryHandler : IRequestHandler<ProductSearchQuery, Result<PagedResultDto<ProductDto>>>
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public ProductSearchQueryHandler(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<PagedResultDto<ProductDto>>> Handle(ProductSearchQuery request, CancellationToken cancellationToken)
        {
            var criteria = request.Criteria ?? new ProductSearchCriteriaDto();

            // Get all products
            var allProducts = await _repository.GetAllAsync(cancellationToken);

            // Apply filters
            var query = allProducts.AsQueryable();

            // Search by term (Name, Description, SKU, Barcode)
            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTerm = criteria.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                    (p.Sku != null && p.Sku.ToLower().Contains(searchTerm)) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm)));
            }

            // Filter by category
            if (criteria.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);
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

            // Apply sorting
            query = criteria.SortBy?.ToLower() switch
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

            return Result<PagedResultDto<ProductDto>>.Success(pagedResult);
        }
    }
}

