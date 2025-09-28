using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Queries.GetById
{
    public class GetProductByIdQuery : IRequest<Result<ProductDetailsDto>>
    {
        public Guid Id { get; set; }
    }
}