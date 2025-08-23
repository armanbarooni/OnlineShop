using MediatR;
using OnlineShop.Application.Contracts.Units;
using System.Collections.Generic;

namespace OnlineShop.Application.Features.Units.Queries.GetUnits
{
    public record GetUnitsQuery() : IRequest<Result<List<UnitDto>>>;
}