using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.GetAll
{
    public class GetAllUserReturnRequestsQuery : IRequest<Result<List<UserReturnRequestDto>>>
    {
    }
}

