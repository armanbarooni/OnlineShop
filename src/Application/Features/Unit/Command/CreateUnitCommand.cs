using MediatR;
using MySqlX.XDevAPI.Common;
using OnlineShop.Application.Common.Models; 

namespace OnlineShop.Application.Features.Unit.Commands.CreateUnit
{
    public class CreateUnitCommand : IRequest<Result<bool>>
    {
        public string Name { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
}
