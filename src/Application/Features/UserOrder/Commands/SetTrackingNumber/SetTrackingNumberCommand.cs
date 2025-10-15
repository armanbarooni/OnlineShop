using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.UserOrder.Commands.SetTrackingNumber
{
    public class SetTrackingNumberCommand : IRequest<Result<bool>>
    {
        public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}
