using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Unit;

namespace OnlineShop.Application.Features.Unit.Command.Create
{
    public class CreateUnitCommand : IRequest<Result<Guid>>
    {
        public  CreateUnitDto? UnitDto { get; set; } 

    }
}
