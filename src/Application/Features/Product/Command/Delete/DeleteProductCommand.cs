using MediatR;
using OnlineShop.Application.Common.Models;
using System;

namespace OnlineShop.Application.Features.Product.Command.Delete
{
    public class DeleteProductCommand : IRequest<Result<string>>
    {
        public DeleteProductCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id{ get; set; }
    }
}
