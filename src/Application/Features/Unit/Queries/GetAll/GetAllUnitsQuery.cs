using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Unit;

namespace OnlineShop.Application.Features.Unit.Queries.GetAll
{
    public record GetAllUnitsQuery : IRequest<Result<List<UnitDto>>>;
}


