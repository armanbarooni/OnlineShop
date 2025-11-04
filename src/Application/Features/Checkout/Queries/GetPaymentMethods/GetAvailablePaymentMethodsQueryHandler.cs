using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Queries.GetPaymentMethods
{
    public class GetAvailablePaymentMethodsQueryHandler : IRequestHandler<GetAvailablePaymentMethodsQuery, Result<IEnumerable<PaymentMethodDto>>>
    {
        public async Task<Result<IEnumerable<PaymentMethodDto>>> Handle(GetAvailablePaymentMethodsQuery request, CancellationToken cancellationToken)
        {
            // Hardcoded payment methods - can be moved to database later
            var paymentMethods = new List<PaymentMethodDto>
            {
                new PaymentMethodDto
                {
                    Id = "online",
                    Name = "پرداخت آنلاین",
                    Description = "پرداخت آنلاین با کارت‌های عضو شتاب",
                    IsAvailable = true,
                    Icon = "credit-card"
                },
                new PaymentMethodDto
                {
                    Id = "cash-on-delivery",
                    Name = "پرداخت در محل",
                    Description = "پرداخت نقدی هنگام تحویل",
                    IsAvailable = true,
                    Icon = "cash"
                },
                new PaymentMethodDto
                {
                    Id = "wallet",
                    Name = "کیف پول",
                    Description = "پرداخت از کیف پول",
                    IsAvailable = true,
                    Icon = "wallet"
                }
            };

            return Result<IEnumerable<PaymentMethodDto>>.Success(paymentMethods);
        }
    }
}

