using MediatR;
using OnlineShop.Application.Common.Models;
using System;

namespace OnlineShop.Application.Features.Product.Command.Delete
{
    public record DeleteProductCommand : IRequest<Result<string>>
    {
        public Guid Id{ get; set; }
    }
}