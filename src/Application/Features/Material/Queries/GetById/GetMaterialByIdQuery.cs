using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Material;

namespace OnlineShop.Application.Features.Material.Queries.GetById
{
    public record GetMaterialByIdQuery : IRequest<Result<MaterialDto>>
    {
        public Guid Id { get; set; }
    }
}
