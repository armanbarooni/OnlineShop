using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Season;

namespace OnlineShop.Application.Features.Season.Commands.Create
{
    public record CreateSeasonCommand : IRequest<Result<SeasonDto>>
    {
        public CreateSeasonDto Season { get; set; } = null!;
    }
}
