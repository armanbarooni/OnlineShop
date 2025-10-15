using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Material;

namespace OnlineShop.Application.Features.Material.Commands.Update
{
    public record UpdateMaterialCommand : IRequest<Result<MaterialDto>>
    {
        public UpdateMaterialDto Material { get; set; } = null!;
    }
}
