using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.Material.Commands.Delete
{
    public record DeleteMaterialCommand(Guid Id) : IRequest<Result<bool>>;
}

