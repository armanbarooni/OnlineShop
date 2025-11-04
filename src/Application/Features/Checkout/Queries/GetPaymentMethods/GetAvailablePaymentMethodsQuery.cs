using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Queries.GetPaymentMethods
{
    public class GetAvailablePaymentMethodsQuery : IRequest<Result<IEnumerable<PaymentMethodDto>>>
    {
    }
}

