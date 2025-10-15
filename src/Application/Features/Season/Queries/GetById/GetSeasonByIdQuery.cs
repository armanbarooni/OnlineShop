using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Season;

namespace OnlineShop.Application.Features.Season.Queries.GetById
{
    public record GetSeasonByIdQuery : IRequest<Result<SeasonDto>>
    {
        public Guid Id { get; set; }
    }
}

