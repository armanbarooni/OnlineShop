using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Features.UserReturnRequest.Queries.Search
{
    public class SearchUserReturnRequestsQuery : IRequest<Result<IEnumerable<UserReturnRequestDto>>>
    {
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}




