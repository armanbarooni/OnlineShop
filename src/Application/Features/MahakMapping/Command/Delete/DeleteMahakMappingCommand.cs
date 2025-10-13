using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.MahakMapping.Command.Delete
{
    public class DeleteMahakMappingCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}

