using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Season;

namespace OnlineShop.Application.Features.Season.Commands.Update
{
    public record UpdateSeasonCommand : IRequest<Result<SeasonDto>>
    {
        public UpdateSeasonDto Season { get; set; } = null!;
    }
}
