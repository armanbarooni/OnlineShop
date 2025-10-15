using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Season;

namespace OnlineShop.Application.Features.Season.Queries.GetAll
{
    public record GetAllSeasonsQuery : IRequest<Result<List<SeasonDto>>>;
}
