using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Domain.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineShop.Application.Features.Product.Queries.GetAll
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PaginatedList<ProductDto>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetAllProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            // Get queryable with includes
            var query = await _productRepository.GetQueryableWithIncludesAsync(cancellationToken);

            // Apply search
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.ProductVariants.Any(v => v.Size.ToLower().Contains(searchTerm) || v.Color.ToLower().Contains(searchTerm))
                );
            }

            // Apply filters
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            if (request.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == request.BrandId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Color))
            {
                query = query.Where(p => p.ProductVariants.Any(v => 
                    v.Color.ToLower().Contains(request.Color.ToLower())));
            }

            if (!string.IsNullOrWhiteSpace(request.Size))
            {
                query = query.Where(p => p.ProductVariants.Any(v => 
                    v.Size.ToLower().Contains(request.Size.ToLower())));
            }

            if (!string.IsNullOrWhiteSpace(request.Material))
            {
                query = query.Where(p => p.ProductMaterials.Any(pm => 
                    pm.Material.Name.ToLower().Contains(request.Material.ToLower())));
            }

            if (!string.IsNullOrWhiteSpace(request.Season))
            {
                query = query.Where(p => p.ProductSeasons.Any(ps => 
                    ps.Season.Name.ToLower().Contains(request.Season.ToLower())));
            }

            // Price range
            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= request.MaxPrice.Value);
            }

            // Stock filter
            if (request.InStockOnly == true)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            // Sorting
            query = request.SortBy?.ToLower() switch
            {
                "price" => request.SortDescending 
                    ? query.OrderByDescending(p => p.Price) 
                    : query.OrderBy(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "name" => request.SortDescending 
                    ? query.OrderByDescending(p => p.Name) 
                    : query.OrderBy(p => p.Name),
                _ => query.OrderBy(p => p.Name) // Default
            };

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtoList = _mapper.Map<List<ProductDto>>(products);

            var paginatedResult = new PaginatedList<ProductDto>(
                dtoList,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            return Result<PaginatedList<ProductDto>>.Success(paginatedResult);
        }
    }
}
