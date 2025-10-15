using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Material;

namespace OnlineShop.Application.Features.Material.Commands.Create
{
    public record CreateMaterialCommand : IRequest<Result<MaterialDto>>
    {
        public CreateMaterialDto Material { get; set; } = null!;
    }
}

