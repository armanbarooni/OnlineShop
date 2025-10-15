using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.Brand.Commands.Delete
{
    public class DeleteBrandCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }

        public DeleteBrandCommand(Guid id)
        {
            Id = id;
        }
    }
}


