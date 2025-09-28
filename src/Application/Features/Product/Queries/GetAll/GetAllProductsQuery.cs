using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;
using System.Collections.Generic;

namespace OnlineShop.Application.Features.Product.Queries.GetAll
{
    public class GetAllProductsQuery : IRequest<Result<List<ProductDto>>>
    {
    }
}