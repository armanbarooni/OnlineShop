using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Brand;

namespace OnlineShop.Application.Features.Brand.Commands.Create
{
    public class CreateBrandCommand : IRequest<Result<BrandDto>>
    {
        public CreateBrandDto Brand { get; set; } = new();
    }
}


