using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Queries.GetCheckoutSummary
{
    public class GetCheckoutSummaryQuery : IRequest<Result<OrderSummaryDto>>
    {
        public Guid UserId { get; set; }
        public Guid? CartId { get; set; }
    }
}

