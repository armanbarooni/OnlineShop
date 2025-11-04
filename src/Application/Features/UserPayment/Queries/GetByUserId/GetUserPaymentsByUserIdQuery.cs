using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Application.DTOs.Common;

namespace OnlineShop.Application.Features.UserPayment.Queries.GetByUserId
{
    public class GetUserPaymentsByUserIdQuery : IRequest<Result<PagedResultDto<UserPaymentDto>>>
    {
        public Guid UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

