using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Material;

namespace OnlineShop.Application.Features.Material.Queries.GetAll
{
    public record GetAllMaterialsQuery : IRequest<Result<List<MaterialDto>>>;
}

