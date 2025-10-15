using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakMapping;

namespace OnlineShop.Application.Features.MahakMapping.Command.Update
{
    public class UpdateMahakMappingCommand : IRequest<Result<MahakMappingDto>>
    {
        public UpdateMahakMappingDto? MahakMapping { get; set; }
    }
}


