using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Commands.UpdateStatus
{
    public class UpdateOrderStatusCommand : IRequest<Result<UserOrderDto>>
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string? UpdatedBy { get; set; }
    }
}


