using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Brand;

namespace OnlineShop.Application.Features.Brand.Queries.GetById
{
    public class GetBrandByIdQuery : IRequest<Result<BrandDto>>
    {
        public Guid Id { get; set; }
    }
}


