using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.Season.Commands.Delete
{
    public record DeleteSeasonCommand(Guid Id) : IRequest<Result<bool>>;
}

