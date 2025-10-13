using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Common;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.Search
{
    public class UserOrderSearchQuery : IRequest<Result<PagedResultDto<UserOrderDto>>>
    {
        public UserOrderSearchCriteriaDto? Criteria { get; set; }
    }
}

