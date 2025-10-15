using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.GetPendingRequests
{
    public class GetPendingUserReturnRequestsQuery : IRequest<Result<List<UserReturnRequestDto>>>
    {
    }
}


