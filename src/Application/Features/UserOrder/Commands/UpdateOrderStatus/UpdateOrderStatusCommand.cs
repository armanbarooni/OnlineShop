using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Enums;

namespace OnlineShop.Application.Features.UserOrder.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommand : IRequest<Result<bool>>
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string? Note { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
