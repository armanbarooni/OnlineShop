using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.OrderStatusHistory;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetOrderTimeline
{
    public class GetOrderTimelineQuery : IRequest<Result<OrderTimelineDto>>
    {
        public Guid OrderId { get; set; }
    }
}
