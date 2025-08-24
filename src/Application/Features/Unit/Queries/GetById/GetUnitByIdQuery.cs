using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Unit;


namespace OnlineShop.Application.Features.Unit.Queries.GetById
{
    public class GetUnitByIdQuery:IRequest<Result<UnitDetailsDto>>
    {
        public Guid Id { get; set; }
    }
}
