using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;
using System.Collections.Generic;

namespace OnlineShop.Application.Features.Product.Queries.GetAll
{
    public class GetAllProductsQuery : IRequest<Result<PaginatedList<ProductDto>>>
    {
        // Search
        public string? SearchTerm { get; set; }

        // Filters
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? Season { get; set; }
        
        // Price Range
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        // Stock
        public bool? InStockOnly { get; set; }
        
        // Sorting
        public string? SortBy { get; set; } // "name", "price", "newest", "popular"
        public bool SortDescending { get; set; }
        
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
