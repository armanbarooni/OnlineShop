using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetByUserId
{
    public class GetUserPaymentsByUserIdQuery : IRequest<Result<IEnumerable<UserPaymentDto>>>
    {
        public Guid UserId { get; set; }
    }
}

