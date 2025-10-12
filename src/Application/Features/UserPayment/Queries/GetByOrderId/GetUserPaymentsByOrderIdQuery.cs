using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetByOrderId
{
    public class GetUserPaymentsByOrderIdQuery : IRequest<Result<IEnumerable<UserPaymentDto>>>
    {
        public Guid OrderId { get; set; }
    }
}
