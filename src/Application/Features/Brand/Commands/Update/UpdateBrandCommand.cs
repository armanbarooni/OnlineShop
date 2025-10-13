using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Brand;

namespace OnlineShop.Application.Features.Brand.Commands.Update
{
    public class UpdateBrandCommand : IRequest<Result<BrandDto>>
    {
        public UpdateBrandDto Brand { get; set; } = new();
    }
}

