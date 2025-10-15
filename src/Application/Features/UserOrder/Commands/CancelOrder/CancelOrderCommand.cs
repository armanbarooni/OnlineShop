using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Commands.CancelOrder
{
    public class CancelOrderCommand : IRequest<Result<UserOrderDto>>
    {
        public Guid OrderId { get; set; }
        public string? CancellationReason { get; set; }
        public string? UpdatedBy { get; set; }
    }
}


