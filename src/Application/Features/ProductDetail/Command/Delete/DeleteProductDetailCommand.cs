using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductDetail.Command.Delete
{
    public class DeleteProductDetailCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}
