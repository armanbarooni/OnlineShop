using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakMapping;

namespace OnlineShop.Application.Features.MahakMapping.Command.Create
{
    public class CreateMahakMappingCommand : IRequest<Result<MahakMappingDto>>
    {
        public CreateMahakMappingDto? MahakMapping { get; set; }
    }
}


