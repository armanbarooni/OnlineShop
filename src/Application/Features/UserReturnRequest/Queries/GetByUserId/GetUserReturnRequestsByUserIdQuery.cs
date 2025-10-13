using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.GetByUserId
{
    public class GetUserReturnRequestsByUserIdQuery : IRequest<Result<List<UserReturnRequestDto>>>
    {
        public Guid UserId { get; set; }
    }
}

