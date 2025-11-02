using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Commands.CalculateShipping
{
    public class CalculateShippingCommand : IRequest<Result<CalculateShippingResultDto>>
    {
        public Guid UserId { get; set; }
        public CalculateShippingRequestDto Request { get; set; } = null!;
    }
}

