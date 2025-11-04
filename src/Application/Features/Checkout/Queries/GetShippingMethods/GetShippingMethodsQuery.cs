using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Queries.GetShippingMethods
{
    public class GetShippingMethodsQuery : IRequest<Result<IEnumerable<ShippingMethodDto>>>
    {
        public Guid? AddressId { get; set; }
        public decimal? OrderWeight { get; set; }
        public decimal? OrderValue { get; set; }
    }
}

